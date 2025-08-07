using API_Maquinas.DTOs;
using API_Maquinas.Models;
using API_Maquinas.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace API_Maquinas.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly StoredContext _context;
        private readonly PdfService _pdfService;

        public SalesController(StoredContext context ,PdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;

        }

        [Authorize(Roles = "admin, vendedor")]
        [HttpPost]
        public async Task<IActionResult> CrearVenta([FromBody] VentaFormDTO ventaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (ventaDto.Items == null || !ventaDto.Items.Any())
            {
                return BadRequest("Debe incluir al menos un producto.");
            }

            Cliente? clienteParaVenta = null;

            if (!string.IsNullOrEmpty(ventaDto.Cliente.Email))
            {
                clienteParaVenta = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Email == ventaDto.Cliente.Email);
            }

            if (clienteParaVenta == null && !string.IsNullOrEmpty(ventaDto.Cliente.Telefono))
            {
                clienteParaVenta = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Telefono == ventaDto.Cliente.Telefono);
            }

            if (clienteParaVenta != null)
            {
                clienteParaVenta.Nombre = ventaDto.Cliente.Nombre;
                clienteParaVenta.Apellido = ventaDto.Cliente.Apellido;
                clienteParaVenta.Telefono = ventaDto.Cliente.Telefono;
                clienteParaVenta.Email = ventaDto.Cliente.Email;
                clienteParaVenta.Direccion = ventaDto.Cliente.Direccion;
                clienteParaVenta.FechaRegistro = ventaDto.Fecha;
                _context.Clientes.Update(clienteParaVenta);
            }
            else
            {
                clienteParaVenta = new Cliente
                {
                    Nombre = ventaDto.Cliente.Nombre,
                    Apellido = ventaDto.Cliente.Apellido,
                    Telefono = ventaDto.Cliente.Telefono,
                    Email = ventaDto.Cliente.Email,
                    Direccion = ventaDto.Cliente.Direccion,
                    //FechaRegistro = DateTime.UtcNow
                };
                _context.Clientes.Add(clienteParaVenta);
            }

            await _context.SaveChangesAsync();


            decimal totalVentaCalculado = 0; 
            var saleItems = new List<SaleItem>();

            foreach (var itemDto in ventaDto.Items)
            {
                var producto = await _context.Maquinas.FindAsync(itemDto.ProductoId);
                if (producto == null)
                    return BadRequest($"Producto con ID {itemDto.ProductoId} no encontrado.");

                if (producto.Stock < itemDto.Cantidad)
                    return BadRequest($"Stock insuficiente para el producto {producto.Marca} {producto.Modelo}. Stock disponible: {producto.Stock}, solicitado: {itemDto.Cantidad}.");

                totalVentaCalculado += producto.PrecioVenta * itemDto.Cantidad;

                producto.Stock -= itemDto.Cantidad;
                _context.Maquinas.Update(producto);

                saleItems.Add(new SaleItem
                {
                    ProductoId = itemDto.ProductoId,
                    Cantidad = itemDto.Cantidad,
                    precioUnitario = producto.PrecioVenta
                });
            }



            
            var venta = new Sales
            {
                Fecha = DateTime.UtcNow, 
                TotalVenta = totalVentaCalculado, 
                ClienteId = clienteParaVenta.Id, 
                Items = saleItems 
            };

            foreach (var item in venta.Items)
            {
                item.Sale = venta; 
            }



            _context.Ventas.Add(venta); 
            
            await _context.SaveChangesAsync();

            return Ok(venta.Id);
        }


        [Authorize(Roles = "admin, vendedor")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> ListarVentas()
        {
            var sales = await _context.Ventas
                  .Include(s => s.Cliente) // IMPORANTÍSIMO: Incluir la información del cliente
                  .Include(s => s.Items)
                      .ThenInclude(si => si.Producto) // Incluir la información del producto para cada ítem de venta
                  .OrderByDescending(s => s.Fecha) // Ordenar por fecha, los más recientes primero
                  .Select(s => new VentaHistorialDTO // Mapear a tu DTO de historial para el frontend
                  {
                      Id = s.Id,
                      // Concatenar Nombre y Apellido del cliente. Si Cliente es nulo, usar "Cliente Desconocido".
                      ClienteNombre = s.Cliente != null ? $"{s.Cliente.Id} / {s.Cliente.Nombre} {s.Cliente.Apellido} " : "Cliente Desconocido",
                      ClienteTelefono = s.Cliente != null ? s.Cliente.Telefono : null,
                      ClienteEmail = s.Cliente != null ? s.Cliente.Email : null,
                      Fecha = s.Fecha,
                      TotalVenta = s.TotalVenta,
                      Items = s.Items.Select(si => new VentaItemHistorialDTO
                      {
                          Producto = si.Producto != null ? $"{si.Producto.Modelo} {si.Producto.Marca}" : "Producto Desconocido",
                          Cantidad = si.Cantidad
                          // Añade aquí PrecioUnitario si tu VentaItemHistorialDTO lo tiene
                      }).ToList()
                  })
                  .ToListAsync();

            return Ok(sales);

        }

        [HttpGet("factura/pdf/{ventaId}")]
        public async Task<IActionResult> GenerarFacturaPdf(int ventaId)
        {
            Sales? venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Items)
                    .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null)
                return NotFound();

            byte[] pdfBytes;

            using (var ms = new MemoryStream())
            {
                var document = new Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Fuente para títulos
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                // Sección Datos Empresa (manual)
                document.Add(new Paragraph("Nombre de la Empresa S.A.", titleFont));
                document.Add(new Paragraph("Dirección: Calle Falsa 123, Ciudad, País", normalFont));
                document.Add(new Paragraph("Teléfono: +54 9 1234 5678", normalFont));
                document.Add(new Paragraph("Email: contacto@empresa.com", normalFont));

                // Línea separadora
                var linea = new Chunk(new iTextSharp.text.pdf.draw.VerticalPositionMark());
                document.Add(new Paragraph("____________________________________________"));

                document.Add(new Paragraph("Factura de Venta", titleFont));
                document.Add(new Paragraph($"ID Venta: {venta.Id}", subtitleFont));
                document.Add(new Paragraph($"Fecha: {venta.Fecha:dd/MM/yyyy}", normalFont));
                document.Add(new Paragraph($"Cliente: {venta.Cliente?.Nombre ?? "Sin nombre"}", normalFont));
                document.Add(new Paragraph($"Teléfono: {venta.Cliente?.Telefono ?? "Sin teléfono"}", normalFont));
                document.Add(new Paragraph($"Email: {venta.Cliente?.Email ?? "Sin email"}", normalFont));
                document.Add(new Paragraph($"Dirección: {venta.Cliente?.Direccion ?? "Sin dirección"}", normalFont));
                document.Add(new Paragraph(" ")); // línea vacía

                // Tabla con productos
                var table = new PdfPTable(3) { WidthPercentage = 100 };
                table.AddCell(new PdfPCell(new Phrase("Producto", subtitleFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Cantidad", subtitleFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Precio Unitario", subtitleFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                foreach (var item in venta.Items)
                {
                    var producto = item.Producto?.Marca ?? "Producto no disponible";
                    var cantidad = item.Cantidad;
                    var precioUnitario = item.precioUnitario;

                    table.AddCell(new PdfPCell(new Phrase(producto, normalFont)));
                    table.AddCell(new PdfPCell(new Phrase(cantidad.ToString(), normalFont)));
                    table.AddCell(new PdfPCell(new Phrase(precioUnitario.ToString("C"), normalFont)));
                }

                document.Add(table);

                document.Add(new Paragraph(" "));
                document.Add(new Paragraph($"Total Venta: {venta.TotalVenta:C}", subtitleFont));

                document.Close();
                pdfBytes = ms.ToArray();
            }

            return File(pdfBytes, "application/pdf", $"Factura_{ventaId}.pdf");
        }
    }
}
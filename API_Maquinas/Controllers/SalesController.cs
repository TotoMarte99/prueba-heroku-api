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
using System.Globalization;

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
                // Actualizar los datos del cliente existente
                clienteParaVenta.Nombre = ventaDto.Cliente.Nombre;
                clienteParaVenta.Apellido = ventaDto.Cliente.Apellido;
                clienteParaVenta.Telefono = ventaDto.Cliente.Telefono;
                clienteParaVenta.Email = ventaDto.Cliente.Email;
                clienteParaVenta.Direccion = ventaDto.Cliente.Direccion;
                clienteParaVenta.FechaRegistro = DateTime.UtcNow; // <--- CORRECCIÓN AQUÍ
                _context.Clientes.Update(clienteParaVenta);
            }
            else
            {
                // Crear un nuevo cliente
                clienteParaVenta = new Cliente
                {
                    Nombre = ventaDto.Cliente.Nombre,
                    Apellido = ventaDto.Cliente.Apellido,
                    Telefono = ventaDto.Cliente.Telefono,
                    Email = ventaDto.Cliente.Email,
                    Direccion = ventaDto.Cliente.Direccion,
                    FechaRegistro = DateTime.UtcNow // <--- CORRECCIÓN AQUÍ
                };
                _context.Clientes.Add(clienteParaVenta);
            }

            // Se guarda el cliente para que tenga un ID antes de crear la venta
            await _context.SaveChangesAsync();

            decimal totalVentaCalculado = 0;
            var saleItems = new List<SaleItem>();

            foreach (var itemDto in ventaDto.Items)
            {
                var producto = await _context.Maquinas.FindAsync(itemDto.ProductoId);
                if (producto == null)
                {
                    return BadRequest($"Producto con ID {itemDto.ProductoId} no encontrado.");
                }

                if (producto.Stock < itemDto.Cantidad)
                {
                    return BadRequest($"Stock insuficiente para el producto {producto.Marca} {producto.Modelo}. Stock disponible: {producto.Stock}, solicitado: {itemDto.Cantidad}.");
                }

                totalVentaCalculado += producto.PrecioVenta * itemDto.Cantidad;

                producto.Stock -= itemDto.Cantidad;
                _context.Maquinas.Update(producto);

                saleItems.Add(new SaleItem
                {
                    ProductoId = itemDto.ProductoId,
                    Cantidad = itemDto.Cantidad,
                    PrecioUnitario = producto.PrecioVenta
                });
            }

            var venta = new Sales
            {
                Fecha = DateTime.UtcNow, // <--- CORRECCIÓN AQUÍ
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
                // Se define el tamaño de la página (A4) y los márgenes para una mejor presentación.
                var document = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // 3. Definición de estilos y fuentes.
                // Se centraliza la creación de fuentes para mantener la consistencia.
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
                var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
                var headerTableFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);

                // 4. Encabezado de la factura con datos de la empresa y la venta.
                // Se usa una tabla para una mejor alineación del contenido.
                var headerTable = new PdfPTable(2) { WidthPercentage = 100 };
                headerTable.SetWidths(new float[] { 3, 2 });

                // Columna 1: Información de la empresa
                var companyCell = new PdfPCell(new Phrase("Maquinarias Miguel", titleFont)) { Border = 0 };
                companyCell.AddElement(new Phrase("Dirección: Ricardo Nuñez 602 - Rosario, Santa Fe, Argentina", normalFont));
                companyCell.AddElement(new Phrase("Teléfono: +54 9 341 610-5083", normalFont));
                companyCell.AddElement(new Phrase("Email: maquinariasmiguel@hotmail.com", normalFont));
                headerTable.AddCell(companyCell);

                // Columna 2: Título y detalles de la factura
                var invoiceCell = new PdfPCell() { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT };
                invoiceCell.AddElement(new Paragraph("FACTURA", titleFont));
                invoiceCell.AddElement(new Paragraph($"Nro. Venta: {venta.Id}", subtitleFont));
                invoiceCell.AddElement(new Paragraph($"Fecha: {venta.Fecha:dd/MM/yyyy}", normalFont));
                headerTable.AddCell(invoiceCell);

                document.Add(headerTable);

                // 5. Línea separadora y datos del cliente.
                document.Add(new Paragraph("--------------------------------------------------------------------------------------------------------------------------------"));
                document.Add(new Paragraph("Datos del Cliente", subtitleFont));
                document.Add(new Paragraph($"Nombre: {venta.Cliente?.Nombre ?? "N/A"}", normalFont));
                document.Add(new Paragraph($"Dirección: {venta.Cliente?.Direccion ?? "N/A"}", normalFont));
                document.Add(new Paragraph($"Teléfono: {venta.Cliente?.Telefono ?? "N/A"}", normalFont));
                document.Add(new Paragraph($"Email: {venta.Cliente?.Email ?? "N/A"}", normalFont));
                document.Add(new Paragraph(" "));

                // 6. Tabla de productos.
                // Se agrega una columna para el subtotal de cada producto.
                var productTable = new PdfPTable(4) { WidthPercentage = 100 };
                productTable.SetWidths(new float[] { 4, 1, 2, 2 });

                // Celdas de encabezado con estilo para mayor visibilidad.
                productTable.AddCell(CreateHeaderCell("Producto", headerTableFont, BaseColor.DARK_GRAY));
                productTable.AddCell(CreateHeaderCell("Cantidad", headerTableFont, BaseColor.DARK_GRAY, Element.ALIGN_CENTER));
                productTable.AddCell(CreateHeaderCell("Precio Unitario", headerTableFont, BaseColor.DARK_GRAY, Element.ALIGN_RIGHT));
                productTable.AddCell(CreateHeaderCell("Subtotal", headerTableFont, BaseColor.DARK_GRAY, Element.ALIGN_RIGHT));

                // Se rellenan los datos de la tabla.
                foreach (var item in venta.Items)
                {
                    var productoNombre = item.Producto?.Marca ?? "Producto no disponible";
                    var subtotal = item.Cantidad * item.PrecioUnitario;

                    productTable.AddCell(CreateDataCell(productoNombre, normalFont));
                    productTable.AddCell(CreateDataCell(item.Cantidad.ToString(), normalFont, Element.ALIGN_CENTER));
                    // Se usa CultureInfo para asegurar el formato de moneda correcto (ej. "$").
                    productTable.AddCell(CreateDataCell(item.PrecioUnitario.ToString("C", new CultureInfo("es-AR")), normalFont, Element.ALIGN_RIGHT));
                    productTable.AddCell(CreateDataCell(subtotal.ToString("C", new CultureInfo("es-AR")), normalFont, Element.ALIGN_RIGHT));
                }
                document.Add(productTable);
                document.Add(new Paragraph(" "));

                // 7. Resumen de totales.
                // Otra tabla para alinear el total a la derecha.
                var totalTable = new PdfPTable(2) { WidthPercentage = 100 };
                totalTable.SetWidths(new float[] { 7, 3 });
                totalTable.AddCell(CreateTotalCell("Total de la Venta:", subtitleFont, Element.ALIGN_RIGHT));
                totalTable.AddCell(CreateTotalCell(venta.TotalVenta.ToString("C", new CultureInfo("es-AR")), subtitleFont, Element.ALIGN_RIGHT));
                document.Add(totalTable);

                document.Close();

                // 8. Retorno del archivo PDF.
                return File(ms.ToArray(), "application/pdf", $"Factura_{ventaId}.pdf");
            }
        }

        // Métodos auxiliares para crear celdas de forma reutilizable y limpia.
        private PdfPCell CreateHeaderCell(string text, Font font, BaseColor color, int alignment = Element.ALIGN_LEFT)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = color,
                HorizontalAlignment = alignment,
                Padding = 5
            };
        }

        private PdfPCell CreateDataCell(string text, Font font, int alignment = Element.ALIGN_LEFT)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = alignment,
                Padding = 5
            };
        }

        private PdfPCell CreateTotalCell(string text, Font font, int alignment)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                Border = 0,
                HorizontalAlignment = alignment,
                Padding = 5
            };
        }
    }
}
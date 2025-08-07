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
using iText.Layout.Properties;

namespace API_Maquinas.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]

    public class MaOrdersController : ControllerBase
    {
        private readonly StoredContext _context;

        public MaOrdersController(StoredContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "usuario,admin,mecanico")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ma_OrderDTO>>> GetOrders()
        {
            var orders = await _context.MaOrders
                .OrderByDescending(o => o.FechaIngreso)
                .Select(o => new Ma_OrderDTO
                {
                    id = o.Id,
                    Nombre = o.Nombre,
                    Apellido = o.Apellido,
                    FechaIngreso = o.FechaIngreso,
                    Telefono = o.Telefono,
                    Email = o.Email,
                    Direccion = o.Direccion,
                    Marca = o.Marca,
                    Modelo = o.Modelo,
                    Accesorios = o.Accesorios,
                    Observaciones = o.Observaciones,
                    CostoFinal = o.CostoFinal,
                    Estado = o.Estado

                }).ToListAsync();

            return Ok(orders);
        }

        [Authorize(Roles = "usuario,admin,mecanico")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Ma_OrderDTO>> GetOrderById(int id)
        {
            var order = await _context.MaOrders
                .Where(o => o.Id == id)
                .Select(o => new Ma_OrderDTO
                {
                    id = o.Id,
                    Nombre = o.Nombre,
                    Apellido = o.Apellido,
                    Telefono = o.Telefono,
                    Email = o.Email,
                    Direccion = o.Direccion,
                    Marca = o.Marca,
                    Modelo = o.Modelo,
                    Accesorios = o.Accesorios,
                    Observaciones = o.Observaciones,
                    FechaIngreso = o.FechaIngreso,
                    Estado = o.Estado,
                    CostoEstimado = o.CostoEstimado,
                    CostoFinal = o.CostoFinal
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound(new { Message = "Orden no encontrada." });
            }

            return Ok(order);
        }

        [Authorize(Roles = "usuario,admin,mecanico")]
        [HttpPost("ordenes")]
        public async Task<IActionResult> CrearOrden([FromBody] Ma_OrderInsertDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var nuevaOrden = new MaOrder
            {
                Nombre = $"{dto.Nombre} {dto.Apellido}".Trim(),
                Telefono = dto.Telefono,
                Observaciones = dto.Observaciones,
                Marca = dto.Marca,
                Email = dto.Email, 
                Direccion = dto.Direccion,
                Modelo = dto.Modelo,
                Accesorios = dto.Accesorios,
                Estado = "Pendiente",
                CostoEstimado = dto.CostoEstimado,
                FechaIngreso = DateTime.UtcNow
            };

            _context.MaOrders.Add(nuevaOrden);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Orden creada con éxito",
                OrdenId = nuevaOrden.Id
            });
        }

        [Authorize(Roles = "admin,mecanico")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrden(int id, [FromBody] MaOrderUpdateDTO dto)
        {
            var orden = await _context.MaOrders.FindAsync(id);

            if (orden == null)
                return NotFound(new { Message = "Orden no encontrada." });

            try
            {
                orden.Estado = dto.Estado;
                orden.CostoFinal = dto.CostoFinal;

                // Con la solución anterior, ya deberías tener esto
                if (orden.FechaIngreso.Kind == DateTimeKind.Local)
                {
                    orden.FechaIngreso = orden.FechaIngreso.ToUniversalTime();
                }

                if (orden.FechaEntrega.HasValue && orden.FechaEntrega.Value.Kind == DateTimeKind.Local)
                {
                    orden.FechaEntrega = orden.FechaEntrega.Value.ToUniversalTime();
                }

                // No es necesario llamar a .Update, EF ya está rastreando el objeto
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Orden actualizada con éxito" });
            }
            catch (DbUpdateConcurrencyException)
            {
                // En caso de que otra persona haya modificado la orden al mismo tiempo
                return Conflict(new { Message = "Error de concurrencia: la orden fue modificada por otro usuario." });
            }
            catch (Exception ex)
            {
                // Captura cualquier otro error y devuelve el mensaje para que podamos depurarlo
                Console.Error.WriteLine($"Error al actualizar la orden ID {id}: {ex.Message}");
                return StatusCode(500, new { Message = $"Error al actualizar orden: {ex.Message}" });
            }
        }



        [Authorize(Roles = "admin,mecanico")]
        [HttpGet("orden/pdf/{id}")]
        public async Task<IActionResult> GenerarOrdenPdf(int id)
        {
            var order = await _context.MaOrders.FindAsync(id);

            if (order == null)
                return NotFound(new { Message = $"No se encontró la orden con ID {id}" });

            byte[] pdfBytes;

            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                var writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                var blueColor = new BaseColor(0, 70, 140);
                var grayColor = new BaseColor(240, 240, 240);

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, blueColor);
                var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, blueColor);
                var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                // ---- SECCIÓN EMPRESA ----
                var empresaTable = new PdfPTable(2) { WidthPercentage = 100 };
                empresaTable.SetWidths(new float[] { 1f, 3f });
                empresaTable.SpacingAfter = 20;

                // Logo
                try
                {
                    string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Logo", "/Logo/logo.jpg");
                    var logo = Image.GetInstance(logoPath);
                    logo.ScaleToFit(100f, 100f);
                    var logoCell = new PdfPCell(logo) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE };
                    empresaTable.AddCell(logoCell);
                }
                catch
                {
                    // Si no se encuentra el logo, poner celda vacía
                    empresaTable.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });
                }

                // Datos empresa
                var empresaInfo = new PdfPCell();
                empresaInfo.Border = Rectangle.NO_BORDER;
                empresaInfo.AddElement(new Paragraph("Maquinarias Miguel", titleFont));
                empresaInfo.AddElement(new Paragraph("Ricardo Nuñez 602", normalFont));
                empresaInfo.AddElement(new Paragraph("Rosario, Argentino", normalFont));
                empresaInfo.AddElement(new Paragraph("Tel: +54 123 456 789", normalFont));
                empresaInfo.AddElement(new Paragraph("Email: maquinariasmiguel@hotmail.com", normalFont));
                empresaTable.AddCell(empresaInfo);

                document.Add(empresaTable);

                // Título principal
                var title = new Paragraph("Orden de Reparación", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                // (el resto igual que antes, tabla orden, cliente, máquina, observaciones, costos...)

                // Tabla resumen orden
                var tableOrden = new PdfPTable(2) { WidthPercentage = 100 };
                tableOrden.SetWidths(new float[] { 1f, 2f });
                tableOrden.SpacingAfter = 15;

                tableOrden.AddCell(new PdfPCell(new Phrase("Número de Orden:", labelFont)) { BackgroundColor = grayColor, Padding = 5 });
                tableOrden.AddCell(new PdfPCell(new Phrase(order.Id.ToString(), normalFont)) { Padding = 5 });

                tableOrden.AddCell(new PdfPCell(new Phrase("Fecha de Ingreso:", labelFont)) { BackgroundColor = grayColor, Padding = 5 });
                tableOrden.AddCell(new PdfPCell(new Phrase(order.FechaIngreso.ToString("dd/MM/yyyy"), normalFont)) { Padding = 5 });

                tableOrden.AddCell(new PdfPCell(new Phrase("Estado:", labelFont)) { BackgroundColor = grayColor, Padding = 5 });
                tableOrden.AddCell(new PdfPCell(new Phrase(order.Estado, normalFont)) { Padding = 5 });

              

                document.Add(tableOrden);

                // Datos Cliente
                var clienteTitle = new Paragraph("Datos del Cliente", subtitleFont);
                clienteTitle.SpacingAfter = 10;
                document.Add(clienteTitle);

                var tableCliente = new PdfPTable(2) { WidthPercentage = 100 };
                tableCliente.SetWidths(new float[] { 1f, 2f });
                tableCliente.SpacingAfter = 15;

                tableCliente.AddCell(new PdfPCell(new Phrase("Nombre:", labelFont)) { BackgroundColor = grayColor, Padding = 5 });
                tableCliente.AddCell(new PdfPCell(new Phrase(order.Nombre, normalFont)) { Padding = 5 });

                tableCliente.AddCell(new PdfPCell(new Phrase("Teléfono:", labelFont)) { BackgroundColor = grayColor, Padding = 5 });
                tableCliente.AddCell(new PdfPCell(new Phrase(order.Telefono ?? "-", normalFont)) { Padding = 5 });

                tableCliente.AddCell(new PdfPCell(new Phrase("Email:", labelFont)) { BackgroundColor = grayColor, Padding = 5 });
                tableCliente.AddCell(new PdfPCell(new Phrase(order.Email ?? "-", normalFont)) { Padding = 5 });

                tableCliente.AddCell(new PdfPCell(new Phrase("Dirección:", labelFont)) { BackgroundColor = grayColor, Padding = 5 });
                tableCliente.AddCell(new PdfPCell(new Phrase(order.Direccion ?? "-", normalFont)) { Padding = 5 });

                document.Add(tableCliente);

                // Detalles Máquina
                var maquinaTitle = new Paragraph("Detalles de la Máquina", subtitleFont);
                maquinaTitle.SpacingAfter = 10;
                document.Add(maquinaTitle);

                var tableMaquina = new PdfPTable(2) { WidthPercentage = 100 };
                tableMaquina.SetWidths(new float[] { 1f, 2f });
                tableMaquina.SpacingAfter = 15;

                tableMaquina.AddCell(new PdfPCell(new Phrase("Marca:", labelFont)) { BackgroundColor = grayColor, Padding = 5 });
                tableMaquina.AddCell(new PdfPCell(new Phrase(order.Marca ?? "-", normalFont)) { Padding = 5 });

                tableMaquina.AddCell(new PdfPCell(new Phrase("Modelo:", labelFont)) { BackgroundColor = grayColor, Padding = 5 });
                tableMaquina.AddCell(new PdfPCell(new Phrase(order.Modelo ?? "-", normalFont)) { Padding = 5 });

                tableMaquina.AddCell(new PdfPCell(new Phrase("Accesorios:", labelFont)) { BackgroundColor = grayColor, Padding = 5 });
                tableMaquina.AddCell(new PdfPCell(new Phrase(order.Accesorios ?? "-", normalFont)) { Padding = 5 });

                document.Add(tableMaquina);

                // Observaciones
                var obsTitle = new Paragraph("Descripción del Problema", subtitleFont);
                obsTitle.SpacingAfter = 10;
                document.Add(obsTitle);

                var obsParagraph = new Paragraph(order.Observaciones ?? "Sin observaciones", normalFont);
                obsParagraph.SpacingAfter = 15;
                document.Add(obsParagraph);

                var tableObs = new PdfPTable(2) { WidthPercentage = 50, HorizontalAlignment = Element.ALIGN_LEFT };
                tableObs.SetWidths(new float[] { 1f, 1f });

                // Costos
                var costosTitle = new Paragraph("Costos", subtitleFont);
                costosTitle.SpacingAfter = 10;
                document.Add(costosTitle);

                var tableCostos = new PdfPTable(2) { WidthPercentage = 50, HorizontalAlignment = Element.ALIGN_LEFT };
                tableCostos.SetWidths(new float[] { 1f, 1f });

                
                tableCostos.AddCell(new PdfPCell(new Phrase("Costo Final:", labelFont)) { BackgroundColor = grayColor, Padding = 5 });
                tableCostos.AddCell(new PdfPCell(new Phrase(order.CostoFinal.HasValue ? order.CostoFinal.Value.ToString("C") : "Pendiente", normalFont)) { Padding = 5 });

                document.Add(tableCostos);

                document.Close();
                pdfBytes = ms.ToArray();
            }

            return File(pdfBytes, "application/pdf", $"Orden_{order.Id}.pdf");
        }


    }
}

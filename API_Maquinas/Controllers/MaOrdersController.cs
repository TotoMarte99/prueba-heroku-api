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
        public byte[] GenerarPdfMejorado(MaOrder order)
        {

            byte[] pdfBytes;

            using (var ms = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 25, 25, 25, 25);
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Fuentes
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK);
                var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
                var grayColor = new BaseColor(64, 64, 64);

                // --- Encabezado ---
                var headerTable = new PdfPTable(2) { WidthPercentage = 100 };
                headerTable.SetWidths(new float[] { 2f, 1f });

                // Celda de datos de la empresa (alineada a la izquierda)
                var empresaCell = new PdfPCell() { Border = Rectangle.NO_BORDER, Padding = 5 };
                empresaCell.AddElement(new Paragraph("Maquinarias Miguel", titleFont));
                empresaCell.AddElement(new Paragraph("Ricardo Nuñez 602", normalFont));
                empresaCell.AddElement(new Paragraph("Rosario, Argentina", normalFont));
                empresaCell.AddElement(new Paragraph("Tel: +54 123 456 789", normalFont));
                empresaCell.AddElement(new Paragraph("Email: maquinariasmiguel@hotmail.com", normalFont));
                headerTable.AddCell(empresaCell);

                // Celda para el título de la orden y número (alineada a la derecha)
                var orderTitleCell = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 };
                orderTitleCell.AddElement(new Paragraph("ORDEN DE REPARACIÓN", subtitleFont));
                orderTitleCell.AddElement(new Paragraph($"Nro. de Orden: {order.Id}", normalFont));
                orderTitleCell.AddElement(new Paragraph($"Fecha de Ingreso: {order.FechaIngreso:dd/MM/yyyy}", normalFont));
                headerTable.AddCell(orderTitleCell);

                document.Add(headerTable);

                // Línea divisoria
                var hr = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.LIGHT_GRAY, Element.ALIGN_CENTER, 1)));
                hr.SpacingAfter = 10;
                document.Add(hr);

                // --- Datos del Cliente ---
                var clienteTitle = new Paragraph("Datos del Cliente", subtitleFont) { SpacingAfter = 5 };
                document.Add(clienteTitle);

                var tableCliente = new PdfPTable(4) { WidthPercentage = 100 };
                tableCliente.SetWidths(new float[] { 1f, 2f, 1f, 2f });
                tableCliente.SpacingAfter = 15;

                // Fila 1: Nombre y Teléfono
                tableCliente.AddCell(CreateLabelCell("Nombre:", labelFont, grayColor));
                tableCliente.AddCell(CreateDataCell(order.Nombre ?? "-", normalFont));
                tableCliente.AddCell(CreateLabelCell("Teléfono:", labelFont, grayColor));
                tableCliente.AddCell(CreateDataCell(order.Telefono ?? "-", normalFont));

                // Fila 2: Email y Dirección
                tableCliente.AddCell(CreateLabelCell("Email:", labelFont, grayColor));
                tableCliente.AddCell(CreateDataCell(order.Email ?? "-", normalFont));
                tableCliente.AddCell(CreateLabelCell("Dirección:", labelFont, grayColor));
                tableCliente.AddCell(CreateDataCell(order.Direccion ?? "-", normalFont));

                document.Add(tableCliente);

                // --- Detalles de la Máquina ---
                var maquinaTitle = new Paragraph("Detalles de la Máquina", subtitleFont) { SpacingAfter = 5 };
                document.Add(maquinaTitle);

                var tableMaquina = new PdfPTable(4) { WidthPercentage = 100 };
                tableMaquina.SetWidths(new float[] { 1f, 2f, 1f, 2f });
                tableMaquina.SpacingAfter = 15;

                // Fila 1: Marca y Modelo
                tableMaquina.AddCell(CreateLabelCell("Marca:", labelFont, grayColor));
                tableMaquina.AddCell(CreateDataCell(order.Marca ?? "-", normalFont));
                tableMaquina.AddCell(CreateLabelCell("Modelo:", labelFont, grayColor));
                tableMaquina.AddCell(CreateDataCell(order.Modelo ?? "-", normalFont));

                // Fila 2: Accesorios
                tableMaquina.AddCell(CreateLabelCell("Accesorios:", labelFont, grayColor));
                var accesoriosCell = new PdfPCell(new Phrase(order.Accesorios ?? "-", normalFont)) { Padding = 5, Colspan = 3 };
                tableMaquina.AddCell(accesoriosCell);

                document.Add(tableMaquina);

                // --- Descripción del Problema / Observaciones ---
                var obsTitle = new Paragraph("Descripción del Problema", subtitleFont) { SpacingAfter = 5 };
                document.Add(obsTitle);
                document.Add(CreateDataCell(order.Observaciones ?? "Sin observaciones", normalFont));

                // --- Costos y Estado ---
                var costosTitle = new Paragraph("Resumen y Costos", subtitleFont) { SpacingAfter = 5 };
                document.Add(costosTitle);

                var tableCostos = new PdfPTable(4) { WidthPercentage = 100 };
                tableCostos.SetWidths(new float[] { 1f, 1.5f, 1f, 1.5f });

                // Fila 1: Estado y Costo Estimado
                tableCostos.AddCell(CreateLabelCell("Estado:", labelFont, grayColor));
                tableCostos.AddCell(CreateDataCell(order.Estado, normalFont));
                tableCostos.AddCell(CreateLabelCell("Costo Estimado:", labelFont, grayColor));
                tableCostos.AddCell(CreateDataCell(order.CostoEstimado.HasValue ? order.CostoEstimado.Value.ToString("C") : "Pendiente", normalFont));

                // Fila 2: Fecha de Entrega y Costo Final
                tableCostos.AddCell(CreateLabelCell("Fecha de Entrega:", labelFont, grayColor));
                tableCostos.AddCell(CreateDataCell(order.FechaEntrega.HasValue ? order.FechaEntrega.Value.ToString("dd/MM/yyyy") : "Pendiente", normalFont));
                tableCostos.AddCell(CreateLabelCell("Costo Final:", labelFont, grayColor));
                tableCostos.AddCell(CreateDataCell(order.CostoFinal.HasValue ? order.CostoFinal.Value.ToString("C") : "Pendiente", normalFont));

                document.Add(tableCostos);

                document.Close();
                pdfBytes = ms.ToArray();
            }

            return pdfBytes;
        }

        // Métodos auxiliares para simplificar la creación de celdas
        private PdfPCell CreateLabelCell(string text, Font font, BaseColor bgColor)
        {
            return new PdfPCell(new Phrase(text, font)) { BackgroundColor = bgColor, Padding = 5, BorderColor = BaseColor.WHITE };
        }

        private PdfPCell CreateDataCell(string text, Font font)
        {
            return new PdfPCell(new Phrase(text, font)) { Padding = 5, BorderColor = BaseColor.WHITE };
        }
    }
}

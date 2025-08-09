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
using System.Globalization;

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
            // Se utiliza AsNoTracking() para optimizar si no se va a modificar el objeto.
            var order = await _context.MaOrders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { Message = $"No se encontró la orden con ID {id}" });

            using (var ms = new MemoryStream())
            {
                // 1. Configuración del documento: Márgenes y tamaño.
                var document = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // 2. Definición de estilos y fuentes centralizados para fácil mantenimiento.
                var mainBlueColor = new BaseColor(0, 70, 140);
                var lightGrayColor = new BaseColor(240, 240, 240);
                var darkGrayColor = new BaseColor(100, 100, 100);

                var mainTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, mainBlueColor);
                var sectionTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, mainBlueColor);
                var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, darkGrayColor);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

                // 2. Encabezado en dos columnas.
                // Se usa una única tabla para toda la sección de encabezado.
                var headerTable = new PdfPTable(2) { WidthPercentage = 100 };
                headerTable.SetWidths(new float[] { 3f, 2f }); // Columna izquierda más ancha para datos de la empresa, derecha para la orden
                headerTable.SpacingAfter = 20;

                // ---- COLUMNA IZQUIERDA: LOGO y Datos de la Empresa ----
                var leftCell = new PdfPCell() { Border = Rectangle.NO_BORDER };

                // Lógica para el logo
             //   var logoParagraph = new Paragraph();
             //   try
             //   {
             //       string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Logo", "logo.jpg");
             //       if (System.IO.File.Exists(logoPath))
              //      {
             //           var logo = Image.GetInstance(logoPath);
              //          logo.ScaleToFit(120f, 120f);
              //          logoParagraph.Add(new Chunk(logo, 0, 0, true));
             //       }
            //    }
           //     catch { /* Manejo de error del logo */ }
          //      leftCell.AddElement(logoParagraph);

                // Datos de la empresa
                leftCell.AddElement(new Paragraph("Maquinarias Miguel", mainTitleFont) { SpacingBefore = 10 });
                leftCell.AddElement(new Paragraph("Ricardo Nuñez 602", normalFont));
                leftCell.AddElement(new Paragraph("Rosario, Santa Fe, Argentina", normalFont));
                leftCell.AddElement(new Paragraph("Tel: +54 9 341 610-5083", normalFont));
                leftCell.AddElement(new Paragraph("Email: maquinariasmiguel@hotmail.com", normalFont));

                headerTable.AddCell(leftCell);

                // ---- COLUMNA DERECHA: Título y Datos de la Orden ----
                var rightCell = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };
                rightCell.AddElement(new Paragraph("ORDEN DE REPARACIÓN", mainTitleFont));
                rightCell.AddElement(new Paragraph($"N°: {order.Id}", sectionTitleFont));
                rightCell.AddElement(new Paragraph($"Fecha de Ingreso: {order.FechaIngreso:dd/MM/yyyy}", normalFont));

                headerTable.AddCell(rightCell);

                document.Add(headerTable);
                document.Add(headerTable);

                // 4. Datos de la Orden, Cliente y Máquina.
                // Se usa una única tabla para agrupar visualmente la información principal.
                var mainInfoTable = new PdfPTable(4) { WidthPercentage = 100 };
                mainInfoTable.SetWidths(new float[] { 1f, 2f, 1f, 2f });
                mainInfoTable.SpacingAfter = 15;

                mainInfoTable.AddCell(CreateLabelCell("Cliente:", labelFont, lightGrayColor));
                mainInfoTable.AddCell(CreateDataCell(order.Nombre ?? "N/A", normalFont, 3)); // Colspan para aprovechar el espacio

                mainInfoTable.AddCell(CreateLabelCell("Teléfono:", labelFont, lightGrayColor));
                mainInfoTable.AddCell(CreateDataCell(order.Telefono ?? "N/A", normalFont));
                mainInfoTable.AddCell(CreateLabelCell("Email:", labelFont, lightGrayColor));
                mainInfoTable.AddCell(CreateDataCell(order.Email ?? "N/A", normalFont));

                mainInfoTable.AddCell(CreateLabelCell("Dirección:", labelFont, lightGrayColor));
                mainInfoTable.AddCell(CreateDataCell(order.Direccion ?? "N/A", normalFont));
                mainInfoTable.AddCell(CreateLabelCell("Estado:", labelFont, lightGrayColor));
                mainInfoTable.AddCell(CreateDataCell(order.Estado ?? "N/A", normalFont));

                // Sección de la máquina
                mainInfoTable.AddCell(CreateLabelCell("Marca:", labelFont, lightGrayColor));
                mainInfoTable.AddCell(CreateDataCell(order.Marca ?? "N/A", normalFont));
                mainInfoTable.AddCell(CreateLabelCell("Modelo:", labelFont, lightGrayColor));
                mainInfoTable.AddCell(CreateDataCell(order.Modelo ?? "N/A", normalFont));

                mainInfoTable.AddCell(CreateLabelCell("Accesorios:", labelFont, lightGrayColor));
                mainInfoTable.AddCell(CreateDataCell(order.Accesorios ?? "N/A", normalFont, 3)); // Colspan para accesorios

                document.Add(mainInfoTable);

                // 5. Descripción del Problema
                // Se utiliza una celda con fondo para destacar la información.
                document.Add(new Paragraph("Descripción del Problema", sectionTitleFont) { SpacingAfter = 10 });
                var obsTable = new PdfPTable(1) { WidthPercentage = 100 };
                obsTable.AddCell(new PdfPCell(new Phrase(order.Observaciones ?? "Sin observaciones", normalFont))
                {
                    Padding = 10,
                    BackgroundColor = lightGrayColor
                });
                document.Add(obsTable);

                // 6. Sección de Costos y Firma
                var footerTable = new PdfPTable(2) { WidthPercentage = 100 };
                footerTable.SetWidths(new float[] { 1f, 1f });
                footerTable.SpacingBefore = 30;

                // Columna de Costos
                var costosCell = new PdfPCell() { Border = Rectangle.NO_BORDER };
                costosCell.AddElement(new Paragraph("Costos", sectionTitleFont));
                var costosTable = new PdfPTable(2) { WidthPercentage = 80 };
                costosTable.AddCell(CreateLabelCell("Costo Final:", labelFont, lightGrayColor));
                costosTable.AddCell(CreateDataCell(order.CostoFinal.HasValue ? order.CostoFinal.Value.ToString("C", new CultureInfo("es-AR")) : "Pendiente", normalFont));
                costosCell.AddElement(costosTable);
                footerTable.AddCell(costosCell);

                // Columna de Firma
                var firmaCell = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER };
                firmaCell.AddElement(new Paragraph(" "));
                firmaCell.AddElement(new Paragraph("__________________________", normalFont) { SpacingBefore = 40 });
                firmaCell.AddElement(new Paragraph("Firma del Cliente", normalFont) { Alignment = Element.ALIGN_CENTER });
                footerTable.AddCell(firmaCell);

                document.Add(footerTable);

                // 6. Sección de Términos y Condiciones
                // Se usa un texto más pequeño para el footer.
                document.Add(new Paragraph("Términos y Condiciones", sectionTitleFont) { SpacingBefore = 30 });
                string terminosTexto = @"
               Maquinarias Miguel no se hará responsable por la pérdida, extravío o hurto del equipo luego de 30 días de haber notificado al cliente la finalización de la reparación o la elaboración del presupuesto. El cliente acepta que el equipo será reparado con repuestos originales o alternativos de igual calidad, según la disponibilidad. El plazo de garantía sobre la reparación es de 90 días, aplicable únicamente a los trabajos realizados y repuestos instalados en esta orden. Pasados los 30 días de la notificación de finalización, la empresa cobrará un cargo diario por almacenaje.

                ";
                var terminosParagraph = new Paragraph(terminosTexto.Trim());
                terminosParagraph.Alignment = Element.ALIGN_JUSTIFIED;
                document.Add(terminosParagraph);


                document.Close();
                var pdfBytes = ms.ToArray();
                return File(pdfBytes, "application/pdf", $"Orden_{order.Id}.pdf");
            }
        }

        // Métodos auxiliares para la creación de celdas.
        private PdfPCell CreateLabelCell(string text, Font font, BaseColor color)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = color,
                Padding = 5,
                Border = Rectangle.BOX,
                BorderColor = BaseColor.LIGHT_GRAY
            };
        }

        private PdfPCell CreateDataCell(string text, Font font, int colspan = 1)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                Padding = 5,
                Colspan = colspan,
                Border = Rectangle.BOX,
                BorderColor = BaseColor.LIGHT_GRAY
            };
        }
    }
}

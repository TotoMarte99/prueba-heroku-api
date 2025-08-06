using API_Maquinas.DTOs;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.IO;

public class PdfService
{
    public byte[] GenerarFacturaPdf(FacturaDto factura)
    {
        using var stream = new MemoryStream();
        var writer = new PdfWriter(stream);
        var pdf = new PdfDocument(writer);
        var doc = new Document(pdf);

        // Cabecera
        doc.Add(new Paragraph("FACTURA")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(20));

        doc.Add(new Paragraph($"Cliente: {factura.NombreCliente} {factura.ApellidoCliente}"));
        doc.Add(new Paragraph($"Email: {factura.EmailCliente}"));
        doc.Add(new Paragraph($"Dirección: {factura.DireccionCliente}"));
        doc.Add(new Paragraph($"Teléfono: {factura.TelefonoCliente}"));
        doc.Add(new Paragraph($"Fecha: {factura.FechaVenta:dd/MM/yyyy}"));
        doc.Add(new Paragraph("\n"));

        // Tabla
        var tabla = new Table(3).UseAllAvailableWidth();
        tabla.AddHeaderCell("Producto");
        tabla.AddHeaderCell("Cantidad");
        tabla.AddHeaderCell("Precio Unitario");

        foreach (var item in factura.Items)
        {
            tabla.AddCell(item.NombreProducto);
            tabla.AddCell(item.Cantidad.ToString());
            tabla.AddCell($"${item.PrecioUnitario:F2}");
        }

        doc.Add(tabla);

        doc.Add(new Paragraph($"\nTOTAL: ${factura.TotalVenta:F2}")
            .SetTextAlignment(TextAlignment.RIGHT)
            .SetFontSize(14));

        doc.Close();

        return stream.ToArray();
    }
}

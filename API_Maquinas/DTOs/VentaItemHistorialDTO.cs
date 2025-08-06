namespace API_Maquinas.DTOs
{
    public class VentaItemHistorialDTO
    {
        public string Producto { get; set; }
        public int Cantidad { get; set; }

        public decimal precioUnitario { get; set; }
        public decimal Subtotal { get; set; }


    }
}

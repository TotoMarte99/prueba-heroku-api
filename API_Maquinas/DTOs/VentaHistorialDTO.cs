namespace API_Maquinas.DTOs
{
    public class VentaHistorialDTO
    {
        public int Id { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }
        public string? ClienteTelefono { get; set; } // Usamos string? para permitir nulos
        public string? ClienteEmail { get; set; }   // Usamos string? para permitir nulos
        public string? ClienteDireccion { get; set; }
        public decimal TotalVenta { get; set; }
        public int Cantidad { get; set; }

        public List<VentaItemHistorialDTO> Items { get; set; } = new();

    }
}

namespace API_Maquinas.Models
{
    public class MaOrder
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Accesorios { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;

        // campos de control de reparación
        public string Estado { get; set; } = "Pendiente"; // Pendiente / En reparación / Finalizado
        public DateTime? FechaEntrega { get; set; }
        public decimal? CostoEstimado { get; set; }
        public decimal? CostoFinal { get; set; }
    }
}

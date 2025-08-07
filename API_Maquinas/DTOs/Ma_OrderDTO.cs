namespace API_Maquinas.DTOs
{
    public class Ma_OrderDTO
    {
        public int id { get; set; }

        // Datos del cliente
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;

        // Datos de la máquina
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Accesorios { get; set; } = string.Empty;

        // Información del problema
        public string Observaciones { get; set; } = string.Empty;

        // Fechas
        public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;
        public DateTime? FechaEntrega { get; set; } = DateTime.UtcNow;

        // Estado y costos
        public string Estado { get; set; } = "Pendiente"; // Pendiente / En reparación / Finalizado
        public decimal? CostoEstimado { get; set; }
        public decimal? CostoFinal { get; set; }
    }

}

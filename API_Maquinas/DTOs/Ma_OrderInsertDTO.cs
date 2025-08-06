namespace API_Maquinas.DTOs
{
    public class Ma_OrderInsertDTO
    {
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

        // Observaciones del cliente / técnico
        public string Observaciones { get; set; } = string.Empty;

        // Opcional
        public decimal? CostoEstimado { get; set; }
    }
}


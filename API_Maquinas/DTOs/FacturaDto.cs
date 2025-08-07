namespace API_Maquinas.DTOs
{
    public class FacturaDto
    {
        public int VentaId { get; set; }
        public string? NombreCliente { get; set; }
        public string? ApellidoCliente { get; set; }
        public string? EmailCliente { get; set; }
        public DateTime FechaVenta { get; set; } = DateTime.UtcNow;
        public decimal TotalVenta { get; set; }
        public List<FacturaItemDto> Items { get; set; } = new List<FacturaItemDto>();
        public string? DireccionCliente { get; set; }
        public string? TelefonoCliente { get; set; }
        
    }
}

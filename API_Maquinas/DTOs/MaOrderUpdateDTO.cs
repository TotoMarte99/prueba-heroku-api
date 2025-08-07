namespace API_Maquinas.DTOs
{
    public class MaOrderUpdateDTO
    {
        public string? Estado { get; set; } // Pendiente / En reparación / Finalizado
        public DateTime? FechaEntrega { get; set; } = DateTime.UtcNow;
        public decimal? CostoFinal { get; set; }
    }
}

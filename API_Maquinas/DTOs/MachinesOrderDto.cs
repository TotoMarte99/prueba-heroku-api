namespace API_Maquinas.DTOs
{
    public class MachinesOrderDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public string? Accesorios { get; set; }
        public string? Observaciones { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public decimal? CostoFinal { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace API_Maquinas.DTOs
{
    public class ClientUpdateDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        public string? Email { get; set; }

        public string? Direccion { get; set; }
    }
}

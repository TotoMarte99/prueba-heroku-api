using System.ComponentModel.DataAnnotations;

namespace API_Maquinas.DTOs
{
    public class ClienteDTO
    {
        public int id { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es requerido.")]
        public string? Nombre { get; set; } = string.Empty; 

        [Required(ErrorMessage = "El apellido del cliente es requerido.")]
        public string? Apellido { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
        public string? Email { get; set; }

        public string? Direccion { get; set; }
    }
}

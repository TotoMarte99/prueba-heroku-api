using System.ComponentModel.DataAnnotations;

namespace API_Maquinas.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; } 
        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; } 

        [MaxLength(200)]
        public string? Direccion { get; set; } 

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}


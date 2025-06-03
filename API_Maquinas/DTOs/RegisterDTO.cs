using API_Maquinas.Validations;
using System.ComponentModel.DataAnnotations;

namespace API_Maquinas.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "La marca no puede superar los 50 caracteres.")]
        [ValidationNotLiteralString(ErrorMessage = "El nombre no puede ser 'string'.")]
        public string Users { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(50, ErrorMessage = "La marca no puede superar los 50 caracteres.")]
        [ValidationNotLiteralString(ErrorMessage = "El nombre no puede ser 'string'.")]
        public string PassWord { get; set; }

        
    }
}

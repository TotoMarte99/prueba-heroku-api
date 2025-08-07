using API_Maquinas.Validations;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace API_Maquinas.DTOs
{
    public class MachineInsertDTO
    {
        [Required(ErrorMessage ="La marca es obligatoria")]
        [StringLength(50, ErrorMessage = "La marca no puede superar los 50 caracteres.")]
        [ValidationNotLiteralString(ErrorMessage = "El nombre no puede ser 'string'.")]
        public string Marca { get; set; }

        [Required(ErrorMessage = "El Modelo es obligatorio")]
        [StringLength(50, ErrorMessage = "El modelo no puede superar los 50 caracteres.")]
        [ValidationNotLiteralString(ErrorMessage = "El nombre no puede ser 'string'.")]
        public string Modelo { get; set; }

        [Required(ErrorMessage = "El Modelo es obligatorio")]
        [StringLength(50, ErrorMessage = "El modelo no puede superar los 50 caracteres.")]
        [ValidationNotLiteralString(ErrorMessage = "El nombre no puede ser 'string'.")]
        public string Tipo { get; set; }



        [Range(1, int.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public int Precio { get; set; }

        public int Stock { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]

        public int PrecioVenta { get; set; }

        public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;

    }
}

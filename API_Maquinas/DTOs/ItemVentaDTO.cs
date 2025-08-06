using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API_Maquinas.DTOs
{
    public class ItemVentaDTO
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")] // Asegura la precisión para valores monetarios
        public decimal PrecioUnitario { get; set; }
    }
}

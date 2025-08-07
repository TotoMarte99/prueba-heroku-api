using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API_Maquinas.Models
{
    public class SaleItem
    {
        [Key]
        public int Id { get; set; }

        public int SaleId { get; set; }

        public int ProductoId { get; set; }

        public int Cantidad { get; set; }

        [ForeignKey("SaleId")]
        public Sales Sale { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] 
        public decimal PrecioUnitario { get; set; }


        [ForeignKey("ProductoId")]
        public Machines Producto { get; set; }
    }
}

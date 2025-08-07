using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace API_Maquinas.Models
{
    public class Sales
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        //olumn(TypeName = "varchar(100)")]
        //blic string ClienteNombre { get; set; }

        public int? ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }


        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalVenta { get; set; }

        public List<SaleItem> Items { get; set; } = new();

    }
}

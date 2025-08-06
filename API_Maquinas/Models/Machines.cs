using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Maquinas.Models
{
    public class Machines
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "varchar(50)")]  
        public string Marca { get; set; }

        [Column(TypeName = "varchar(50)")]  
        public string Modelo { get; set; }

        [Column(TypeName = "varchar(50)")]  
        public string Tipo { get; set; }

        [Column(TypeName = "int")]
        public int Precio { get; set; }
        [Column(TypeName = "int")]

        public int Stock { get; set; }
        [Column(TypeName = "int")]

        public int PrecioVenta { get; set; }

        public DateTime FechaIngreso { get; set; }




    }
}

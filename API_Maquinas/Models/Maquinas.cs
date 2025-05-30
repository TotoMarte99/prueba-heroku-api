using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Maquinas.Models
{
    public class Maquinas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "varchar(50)")]  //Asi defino el tipo de valor que entra en la tabla.
        public string Marca { get; set; }

        [Column(TypeName = "varchar(50)")]  //Asi defino el tipo de valor que entra en la tabla.
        public string Modelo { get; set; }

        [Column(TypeName = "varchar(50)")]  //Asi defino el tipo de valor que entra en la tabla.
        public string Tipo { get; set; }

        [Column(TypeName = "int")]
        public int Precio { get; set; }

        

    }
}

namespace API_Maquinas.DTOs
{
    public class MachineDTO
    {
        public int id { get; set; }
        public string Marca { get; set; }
        public string Modelo { get;set; }
        public string Categoria { get; set; }
        public string Tipo { get; set; }

        public int Precio { get; set; }
        public int Stock { get; set; }
        public int PrecioVenta { get; set; }

        public DateTime FechaIngreso { get; set; }


    }
}

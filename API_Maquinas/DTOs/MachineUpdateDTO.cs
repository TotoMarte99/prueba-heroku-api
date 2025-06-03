namespace API_Maquinas.DTOs
{
    public class MachineUpdateDTO
    {
        public int id { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Tipo { get; set; }
        public int Precio { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace API_Maquinas.DTOs
{
    public class VentaFormDTO
    {
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    // public string ClienteNombre { get; set; }

        // AÑADIR esta propiedad para el cliente
        [Required(ErrorMessage = "Los datos del cliente son requeridos.")]
        public ClienteDTO Cliente { get; set; } = new ClienteDTO(); 
        public List<ItemVentaDTO> Items { get; set; } = new();
    }
}

using API_Maquinas.Controllers;
using API_Maquinas.DTOs;

namespace API_Maquinas.Services
{
    public interface ILoginAuth
    {
        Task<LoginResponseDTO?> GetUsers(string username, string password);
        Task<bool> RegisterAsync(RegisterDTO dto);
    }
}

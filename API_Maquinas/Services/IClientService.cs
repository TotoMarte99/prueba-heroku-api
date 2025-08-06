using API_Maquinas.DTOs;

namespace API_Maquinas.Services
{
    public interface IClientService
    {
        Task<IEnumerable<ClienteDTO>> GetClient();
        Task<ClienteDTO> GetMClientID(int id);
        Task<ClienteDTO> Update(int id, ClientUpdateDTO clientUpdate);
        Task<ClienteDTO> Add(ClientInsertDTO clientInsert);
        Task<IEnumerable<ClienteDTO>> Search(string marca);
        Task<ClienteDTO> Delete(int id);
    }
}

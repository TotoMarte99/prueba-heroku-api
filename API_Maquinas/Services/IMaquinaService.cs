using API_Maquinas.DTOs;

namespace API_Maquinas.Services
{
    public interface IMaquinaService
    {
        Task<IEnumerable<MaquinaDTO>> GetMaquina();
        Task<MaquinaDTO> GetMaquinaID(int id);
        Task<MaquinaDTO> Update(int id, MaquinaUpdateDTO maquinaUpdate);
        Task<MaquinaDTO> Add(MaquinaInsertDTO maquinaInsert);
        Task<IEnumerable<MaquinaDTO>> Search(string marca);
        Task<MaquinaDTO> Delete(int id);
    }
}

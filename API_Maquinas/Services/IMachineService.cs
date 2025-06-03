using API_Maquinas.DTOs;

namespace API_Maquinas.Services
{
    public interface IMachineService
    {
        Task<IEnumerable<MachineDTO>> GetMaquina();
        Task<MachineDTO> GetMaquinaID(int id);
        Task<MachineDTO> Update(int id, MachineUpdateDTO maquinaUpdate);
        Task<MachineDTO> Add(MachineInsertDTO maquinaInsert);
        Task<IEnumerable<MachineDTO>> Search(string marca);
        Task<MachineDTO> Delete(int id);
    }
}

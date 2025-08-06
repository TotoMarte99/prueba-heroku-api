using API_Maquinas.DTOs;
using API_Maquinas.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Maquinas.Services
{
    public class ClientService : IClientService
    {
        StoredContext _context;

        public ClientService(StoredContext context)
        {
            _context = context;
        }

        public async Task<ClienteDTO> Add(ClientInsertDTO clientInsert)
        {

            var cliente = new Cliente()
            {
                Nombre = clientInsert.Nombre,
                Apellido = clientInsert.Apellido,
                Email = clientInsert.Email,
                Direccion = clientInsert.Direccion,
                Telefono = clientInsert.Telefono,
               
            };

            await _context.AddRangeAsync(cliente);
            await _context.SaveChangesAsync();

            var clienteDTO = new ClienteDTO
            {
                id = cliente.Id,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Email = cliente.Email,
                Direccion = cliente.Direccion,
                Telefono = cliente.Telefono,
                
            };

            return clienteDTO;
        }

        public async Task<ClienteDTO> Delete(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente != null)
            {
                var clienteDto = new ClienteDTO
                {
                    id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    Email = cliente.Email,
                    Direccion = cliente.Direccion,
                    Telefono = cliente.Telefono,
                };

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();

                return clienteDto;
            }

            return null;
        }

        public async Task<IEnumerable<ClienteDTO>> GetClient()
        {
            return await _context.Clientes.Select(b => new ClienteDTO
            {
                id = b.Id,
                Nombre = b.Nombre,
                Apellido = b.Apellido,
                Telefono = b.Telefono,
                Direccion = b.Direccion,
                Email = b.Email,
            }).ToListAsync();
        }

        public async Task<ClienteDTO> GetMClientID(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente != null)
            {
                var clienteDto = new ClienteDTO
                {
                    id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    Telefono = cliente.Telefono,
                    Direccion = cliente.Direccion,
                    Email = cliente.Email,
                    
                };

                return clienteDto;
            }

            return null;
        }

        public async Task<IEnumerable<ClienteDTO>> Search(string apellido)
        {
            return await _context.Clientes
                           .Where(m => m.Apellido.Contains(apellido))
                           .Select(m => new ClienteDTO
                           {
                               id = m.Id,
                               Nombre = m.Nombre,
                               Apellido = m.Apellido,
                               Telefono = m.Telefono,
                               Direccion = m.Direccion,
                               Email = m.Email,
                           })
                           .ToListAsync();
        }

        public async Task<ClienteDTO> Update(int id, ClientUpdateDTO clienteUpdate)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente != null)
            {

                cliente.Id = clienteUpdate.Id;
                cliente.Nombre = clienteUpdate.Nombre;
                cliente.Apellido = clienteUpdate.Apellido;
                cliente.Telefono = clienteUpdate.Telefono;
                cliente.Email = clienteUpdate.Email;
                cliente.Direccion = clienteUpdate.Direccion;
               

                _context.Clientes.Update(cliente);
                await _context.SaveChangesAsync();

                var clienteDto = new ClienteDTO
                {
                    id = cliente.Id,
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    Telefono = cliente.Telefono,
                    Email = cliente.Email,
                    Direccion = cliente.Direccion,
                    
                };
                return clienteDto;

            }

            return null;
        }
    }
}

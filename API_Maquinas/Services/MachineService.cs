
using API_Maquinas.DTOs;
using API_Maquinas.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.RegularExpressions;

namespace API_Maquinas.Services
{
    public class MachineService : IMachineService
    {
        StoredContext _context;

        public MachineService(StoredContext context)
        {
            _context = context;
        }
        public async Task<MachineDTO> Add(MachineInsertDTO maquinaInsert)
        {
            var maquina = new Machines()
            {
                Marca = maquinaInsert.Marca,
                Modelo = maquinaInsert.Modelo,
                Tipo = maquinaInsert.Tipo,
                Precio = maquinaInsert.Precio
            };

            await _context.AddRangeAsync(maquina);
            await _context.SaveChangesAsync();

            var maquinaDto = new MachineDTO
            {
                id = maquina.Id,
                Marca = maquina.Marca,
                Modelo = maquina.Modelo,
                Tipo = maquina.Tipo,
                Precio = maquina.Precio,
            };

            return maquinaDto;
        }

        public async Task<IEnumerable<MachineDTO>> Search(string maquina)
        {
            return await _context.Maquinas
                .Where(m => m.Marca.Contains(maquina))
                .Select(m => new MachineDTO
                {
                    id = m.Id,
                    Marca = m.Marca,
                    Modelo = m.Modelo,
                    Tipo = m.Tipo,
                    Precio = m.Precio
                })
                .ToListAsync();

            
        }

        public async Task<MachineDTO> Delete(int id)
        {
            var maquina = await _context.Maquinas.FindAsync(id);

            if (maquina != null)
            {
                var maquinaDto = new MachineDTO
                {
                    id = maquina.Id,
                    Marca = maquina.Marca,
                    Modelo = maquina.Modelo,
                    Tipo = maquina.Tipo,
                    Precio = maquina.Precio,
                };

                _context.Maquinas.Remove(maquina);
                await _context.SaveChangesAsync();

                return maquinaDto;
            }

            return null;
        }

        public async Task<IEnumerable<MachineDTO>> GetMaquina()
        {

            return await _context.Maquinas.Select(b => new MachineDTO
            {
                id = b.Id,
                Marca = b.Marca,
                Modelo = b.Modelo,
                Tipo = b.Tipo,
                Precio = b.Precio,
            }).ToListAsync();
            
        }

        public async Task<MachineDTO> GetMaquinaID(int id)
        {
            var maquina = await _context.Maquinas.FindAsync(id);

            if(maquina != null)
            {
                var maquinaDto = new MachineDTO
                {
                    id = maquina.Id,
                    Marca = maquina.Marca,
                    Modelo = maquina.Modelo,
                    Tipo = maquina.Tipo,
                    Precio = maquina.Precio,
                };

                return maquinaDto;
            }

            return null;
        }

       

        public async Task<MachineDTO> Update(int id, MachineUpdateDTO maquinaUpdate)
        {
            var maquina = await _context.Maquinas.FindAsync(id);

            if (maquina != null)
            {

                maquina.Id = maquinaUpdate.id;
                maquina.Marca = maquinaUpdate.Marca;
                maquina.Modelo = maquinaUpdate.Modelo;
                maquina.Tipo = maquinaUpdate.Tipo;
                maquina.Precio = maquinaUpdate.Precio;

                _context.Maquinas.Update(maquina);
                await _context.SaveChangesAsync();

                var maquinaDto = new MachineDTO
                {
                    id = maquina.Id,
                    Marca = maquina.Marca,
                    Modelo = maquina.Modelo,
                    Tipo = maquina.Tipo,
                    Precio = maquina.Precio,
                };
                return maquinaDto;

            }

            return null;
        }

        

        private class StoreContext
        {
        }
    }
}

using API_Maquinas.DTOs;
using API_Maquinas.Models;
using API_Maquinas.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace API_Maquinas.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]

    public class MachinesController : ControllerBase
    {
        private IMachineService maqservice;

        public MachinesController(StoredContext context, IMachineService _maqservice)
        {
            maqservice = _maqservice;

        }

        [HttpGet]
        public async Task<ActionResult<MachineDTO>> GetMaquina()
        {
            var maquinas = await maqservice.GetMaquina();

            return Ok(maquinas);
        }


        //[Authorize(Roles = "usuario,admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Machines>> GetMaquinaID(int id)
        {
            var maquina = await maqservice.GetMaquinaID(id);

            if (maquina == null)
                return NotFound();

            return Ok(maquina);
        }

        [Authorize(Roles = "usuario,admin")]
        [HttpGet("buscar/marca/{marca}")]
        public async Task<ActionResult<Machines>> GetMaquinaMarca(string marca)
        {
            var maquina = await maqservice.Search(marca);

            if(!marca.Any())
            {
                return NotFound();
            }

            return Ok(maquina);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<Machines>> Add(MachineInsertDTO maquinaInsert)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var maquinaDto = await maqservice.Add(maquinaInsert);

            return CreatedAtAction(nameof(GetMaquinaID), new { id = maquinaDto.id }, maquinaDto);

        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{Id}")]

        public async Task<ActionResult<Machines>> Delete(int Id)
        {
            var maquina = await maqservice.Delete(Id);

            if (maquina == null)
            {
                NotFound();
                
            }
            else
            {
                Ok(maquina);
            }

            return null;
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Machines>> Update(int id, MachineUpdateDTO maquinaUpdate)
        {
            var maquina = await maqservice.Update(id, maquinaUpdate);

            if(maquina == null)
            {
                NotFound();
            }

            return Ok(maquina);

        }

       
    }
}

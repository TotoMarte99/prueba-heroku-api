using API_Maquinas.DTOs;
using API_Maquinas.Models;
using API_Maquinas.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Maquinas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaquinaController : ControllerBase
    {
        private StoredContext _context;
        private IMaquinaService maqservice;

        public MaquinaController(StoredContext context, IMaquinaService _maqservice)
        {
            _context = context;
            maqservice = _maqservice;

        }

        [HttpGet]
        public async Task<ActionResult<MaquinaDTO>> GetMaquina()
        {
            var maquinas = await maqservice.GetMaquina();

            return Ok(maquinas);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Maquinas>> GetMaquinaID(int id)
        {
            var maquina = await maqservice.GetMaquinaID(id);

            if (maquina == null)
                return NotFound();

            return Ok(maquina);
        }

        [HttpGet("buscar/marca/{marca}")]
        public async Task<ActionResult<Maquinas>> GetMaquinaMarca(string marca)
        {
            var maquina = await maqservice.Search(marca);

            if(!marca.Any())
            {
                return NotFound();
            }

            return Ok(maquina);
        }


        [HttpPost]
        public async Task<ActionResult<Maquinas>> Add(MaquinaInsertDTO maquinaInsert)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var maquinaDto = await maqservice.Add(maquinaInsert);

            return CreatedAtAction(nameof(GetMaquinaID), new { id = maquinaDto.id }, maquinaDto);

        }

        [HttpDelete("{Id}")]

        public async Task<ActionResult<Maquinas>> Delete(int Id)
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

        [HttpPut("{id}")]
        public async Task<ActionResult<Maquinas>> Update(int id, MaquinaUpdateDTO maquinaUpdate)
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

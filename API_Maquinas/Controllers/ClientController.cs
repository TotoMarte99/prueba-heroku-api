using API_Maquinas.DTOs;
using API_Maquinas.Models;
using API_Maquinas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_Maquinas.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [Authorize(Roles = "admin")]

        [HttpGet]
        public async Task<ActionResult<ClienteDTO>> GetClient()
        {
            var clientes = await _clientService.GetClient();

            return Ok(clientes);
        }


       [Authorize(Roles = "admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetMClientID(int id)
        {
            var cliente = await _clientService.GetMClientID(id);

            if (cliente == null)
                return NotFound();

            return Ok(cliente);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("buscar/cliente/{apellido}")]
        public async Task<ActionResult<Cliente>> GetCliente(string apellido)
        {
            var clientes = await _clientService.Search(apellido);

            if (!apellido.Any())
            {
                return NotFound();
            }

            return Ok(clientes);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<Cliente>> Add(ClientInsertDTO clientInsert)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cliente = await _clientService.Add(clientInsert);

            return CreatedAtAction(nameof(GetClient), new { id = cliente.id }, cliente);

        }

        //[Authorize(Roles = "admin")]
        //[HttpDelete("{Id}")]

        //public async Task<ActionResult<Cliente>> Delete(int Id)
        //{
        //    var cliente = await _clientService.Delete(Id);

        //    if (cliente == null)
        //    {
        //        NotFound();

        //    }
        //    else
        //    {
        //        Ok(cliente);
        //    }

        //    return NoContent();
        //}

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Cliente>> Update(int id, ClientUpdateDTO clientUpdate)
        {
            var cliente = await _clientService.Update(id, clientUpdate);

            if (cliente == null)
            {
                NotFound();
            }

            return Ok(cliente);

        }


    }
}


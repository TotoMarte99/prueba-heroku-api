using API_Maquinas.DTOs;
using API_Maquinas.Models;
using API_Maquinas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace API_Maquinas.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class Users : ControllerBase
    {
        private ILoginAuth loginAuth;

        public Users(ILoginAuth loginAuth)
        {
            this.loginAuth = loginAuth;
        }

       
        [HttpPost("login")]
        public async Task<ActionResult<Logins>> GetUsers(string username, string password)
        {
            var token = await loginAuth.GetUsers(username, password);

            if (token == null)
                return Unauthorized("Credenciales inválidas");

            return Ok(new { token });
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            var result = await loginAuth.RegisterAsync(dto);
            if (!result)
                return BadRequest("El usuario ya existe.");

            return Ok("Usuario registrado correctamente.");
        }


    }
}

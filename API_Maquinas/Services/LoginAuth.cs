using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API_Maquinas.DTOs;
using API_Maquinas.Models;
using BCrypt.Net;

namespace API_Maquinas.Services
{
    public class LoginAuth : ILoginAuth
    {
        private readonly IConfiguration _configuration;
        private StoredContext _context;

        public LoginAuth(StoredContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponseDTO?> GetUsers(string username, string password)
        {
            try
            {
                var user = await _context.Logins
                    .FirstOrDefaultAsync(u => u.Users == username);

                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PassWord))
                {
                    return null;
                }

                var claims = new[]
                {
            new Claim(ClaimTypes.Name, user.Users),
            new Claim(ClaimTypes.Role, user.Rol ?? "usuario")
        };

                var jwtKey = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                    throw new InvalidOperationException("La clave JWT no está configurada.");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: creds);

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return new LoginResponseDTO
                {
                    Username = user.Users,
                    Token = tokenString
                };
            }
            catch (Exception ex)
            {
                // Log the specific exception message.
                Console.Error.WriteLine($"EXCEPCIÓN en GetUsers: {ex.Message}");
                // Rethrow the exception to be handled by the controller.
                throw;
            }
        }

        public async Task<bool> RegisterAsync(RegisterDTO dto)
        {
            var user_exist = await _context.Logins.AnyAsync(u => u.Users == dto.Users);
            if (user_exist)
                return false;

            // 1. 🚨 CORRECCIÓN: Guardar la contraseña hasheada, no la de texto plano.
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.PassWord);

            var newUser = new Logins
            {
                Users = dto.Users,
                PassWord = hashedPassword, // 🚨 CORRECCIÓN: Asignar el hash aquí
                Rol = "usuario"
            };

            _context.Logins.Add(newUser);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
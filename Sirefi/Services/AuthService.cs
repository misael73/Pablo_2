using Microsoft.EntityFrameworkCore;
using Sirefi.Data;
using Sirefi.DTOs;
using Sirefi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace Sirefi.Services;

public class AuthService : IAuthService
{
    private readonly FormsDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public AuthService(FormsDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _httpClient = new HttpClient();
    }

    public async Task<LoginResponse> ValidateGoogleToken(string idToken)
    {
        try
        {
            // Decode JWT token to get user info
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(idToken) as JwtSecurityToken;

            if (jsonToken == null)
            {
                return new LoginResponse { Success = false, Error = "Token inválido" };
            }

            var email = jsonToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = jsonToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var picture = jsonToken.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;
            var sub = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return new LoginResponse { Success = false, Error = "No se pudo obtener el correo del token" };
            }

            var userInfo = new GoogleUserInfo
            {
                Sub = sub ?? "",
                Email = email,
                Name = name ?? "",
                Picture = picture ?? ""
            };

            // Create or update user
            var user = await CreateOrUpdateUser(userInfo);

            return new LoginResponse
            {
                Success = true,
                Usuario = new UserDto
                {
                    Id = user.Id,
                    Nombre = user.Nombre,
                    Email = user.Correo,
                    Foto = user.Foto,
                    Rol = user.Rol
                }
            };
        }
        catch (Exception ex)
        {
            return new LoginResponse { Success = false, Error = $"Error al validar token: {ex.Message}" };
        }
    }

    public async Task<Usuario?> GetUserByEmail(string email)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == email);
    }

    public async Task<Usuario> CreateOrUpdateUser(GoogleUserInfo userInfo)
    {
        var existingUser = await GetUserByEmail(userInfo.Email);

        if (existingUser != null)
        {
            // Update user info
            existingUser.Nombre = userInfo.Name;
            existingUser.Foto = userInfo.Picture;
            existingUser.UltimoAcceso = DateTime.Now;
            existingUser.FechaActualizacion = DateTime.Now;
            
            await _context.SaveChangesAsync();
            return existingUser;
        }

        // Create new user
        var newUser = new Usuario
        {
            Correo = userInfo.Email,
            Nombre = userInfo.Name,
            Foto = userInfo.Picture,
            Rol = "reportante", // Default role
            Activo = true,
            FechaCreacion = DateTime.Now,
            UltimoAcceso = DateTime.Now
        };

        _context.Usuarios.Add(newUser);
        await _context.SaveChangesAsync();

        return newUser;
    }
}

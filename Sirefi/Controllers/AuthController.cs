using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Sirefi.DTOs;
using Sirefi.Services;

namespace Sirefi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("google")]
    public async Task<IActionResult> ValidateGoogle([FromForm] string id_token)
    {
        if (string.IsNullOrEmpty(id_token))
        {
            return BadRequest(new { success = false, error = "Token no proporcionado" });
        }

        var result = await _authService.ValidateGoogleToken(id_token);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        // Create claims for authentication
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, result.Usuario!.Id.ToString()),
            new Claim(ClaimTypes.Name, result.Usuario.Nombre),
            new Claim(ClaimTypes.Email, result.Usuario.Email),
            new Claim(ClaimTypes.Role, result.Usuario.Rol)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Ok(new { success = true, message = "Sesión cerrada correctamente" });
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Unauthorized(new { success = false, error = "No autenticado" });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            success = true,
            usuario = new
            {
                id = userId,
                nombre = userName,
                email = userEmail,
                rol = userRole
            }
        });
    }
}

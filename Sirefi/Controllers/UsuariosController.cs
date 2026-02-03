using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sirefi.Data;
using Sirefi.DTOs;
using Sirefi.Models;

namespace Sirefi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly FormsDbContext _context;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(FormsDbContext context, ILogger<UsuariosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioDto>>>> GetUsuarios(
        [FromQuery] bool? activo = null,
        [FromQuery] string? rol = null)
    {
        try
        {
            var query = _context.Usuarios.AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(u => u.Activo == activo.Value);
            }

            if (!string.IsNullOrWhiteSpace(rol))
            {
                query = query.Where(u => u.Rol.ToLower() == rol.ToLower());
            }

            var usuarios = await query
                .OrderBy(u => u.Nombre)
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    Rol = u.Rol,
                    Telefono = u.Telefono,
                    Foto = u.Foto,
                    Departamento = u.Departamento,
                    Activo = u.Activo,
                    UltimoAcceso = u.UltimoAcceso,
                    FechaCreacion = u.FechaCreacion
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<UsuarioDto>>.Ok(usuarios, "Usuarios obtenidos correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios");
            return StatusCode(500, ApiResponse<IEnumerable<UsuarioDto>>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get a user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> GetUsuario(int id)
    {
        try
        {
            var usuario = await _context.Usuarios
                .Where(u => u.Id == id)
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    Rol = u.Rol,
                    Telefono = u.Telefono,
                    Foto = u.Foto,
                    Departamento = u.Departamento,
                    Activo = u.Activo,
                    UltimoAcceso = u.UltimoAcceso,
                    FechaCreacion = u.FechaCreacion
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound(ApiResponse<UsuarioDto>.Fail("Usuario no encontrado"));
            }

            return Ok(ApiResponse<UsuarioDto>.Ok(usuario, "Usuario obtenido correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario {Id}", id);
            return StatusCode(500, ApiResponse<UsuarioDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get a user by email
    /// </summary>
    [HttpGet("por-correo/{correo}")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> GetUsuarioPorCorreo(string correo)
    {
        try
        {
            var usuario = await _context.Usuarios
                .Where(u => u.Correo.ToLower() == correo.ToLower())
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    Rol = u.Rol,
                    Telefono = u.Telefono,
                    Foto = u.Foto,
                    Departamento = u.Departamento,
                    Activo = u.Activo,
                    UltimoAcceso = u.UltimoAcceso,
                    FechaCreacion = u.FechaCreacion
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound(ApiResponse<UsuarioDto>.Fail("Usuario no encontrado"));
            }

            return Ok(ApiResponse<UsuarioDto>.Ok(usuario, "Usuario obtenido correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario por correo {Correo}", correo);
            return StatusCode(500, ApiResponse<UsuarioDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get technicians (users with role 'tecnico' or 'administrador')
    /// </summary>
    [HttpGet("tecnicos")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioDto>>>> GetTecnicos()
    {
        try
        {
            var tecnicos = await _context.Usuarios
                .Where(u => u.Activo && (u.Rol.ToLower() == "tecnico" || u.Rol.ToLower() == "administrador"))
                .OrderBy(u => u.Nombre)
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    Rol = u.Rol,
                    Departamento = u.Departamento,
                    Activo = u.Activo,
                    FechaCreacion = u.FechaCreacion
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<UsuarioDto>>.Ok(tecnicos, "Técnicos obtenidos correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener técnicos");
            return StatusCode(500, ApiResponse<IEnumerable<UsuarioDto>>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> CreateUsuario([FromBody] CreateUsuarioDto dto)
    {
        try
        {
            // Validate rol
            var validRoles = new[] { "reportante", "tecnico", "administrador" };
            if (!validRoles.Contains(dto.Rol?.ToLower()))
            {
                return BadRequest(ApiResponse<UsuarioDto>.Fail("Rol inválido. Debe ser: reportante, tecnico o administrador"));
            }

            // Check if email already exists
            var exists = await _context.Usuarios
                .AnyAsync(u => u.Correo.ToLower() == dto.Correo.ToLower());

            if (exists)
            {
                return BadRequest(ApiResponse<UsuarioDto>.Fail($"Ya existe un usuario con el correo '{dto.Correo}'"));
            }

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                Rol = dto.Rol ?? "reportante",
                Telefono = dto.Telefono,
                Foto = dto.Foto,
                Departamento = dto.Departamento,
                Activo = dto.Activo,
                FechaCreacion = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var result = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                Telefono = usuario.Telefono,
                Foto = usuario.Foto,
                Departamento = usuario.Departamento,
                Activo = usuario.Activo,
                FechaCreacion = usuario.FechaCreacion
            };

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id },
                ApiResponse<UsuarioDto>.Ok(result, "Usuario creado correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario");
            return StatusCode(500, ApiResponse<UsuarioDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Create or get user from Google OAuth
    /// </summary>
    [HttpPost("google-login")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> GoogleLogin([FromBody] GoogleLoginDto dto)
    {
        try
        {
            // In a real implementation, you would validate the Google token
            // For now, we'll extract the user info from the token claims
            // This is a simplified version - in production, use Google.Apis.Auth
            
            // Decode JWT token (simplified - in production use proper validation)
            var parts = dto.IdToken.Split('.');
            if (parts.Length != 3)
            {
                return BadRequest(ApiResponse<UsuarioDto>.Fail("Token de Google inválido"));
            }

            // Decode payload
            var payload = parts[1];
            var paddedPayload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var jsonBytes = Convert.FromBase64String(paddedPayload.Replace('-', '+').Replace('_', '/'));
            var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
            var claims = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (claims == null || !claims.ContainsKey("email"))
            {
                return BadRequest(ApiResponse<UsuarioDto>.Fail("No se pudo obtener información del usuario"));
            }

            var email = claims["email"]?.ToString() ?? "";
            var name = claims.GetValueOrDefault("name")?.ToString() ?? email;
            var picture = claims.GetValueOrDefault("picture")?.ToString();

            // Find or create user
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo.ToLower() == email.ToLower());

            if (usuario == null)
            {
                usuario = new Usuario
                {
                    Nombre = name,
                    Correo = email,
                    Rol = "reportante",
                    Foto = picture,
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    UltimoAcceso = DateTime.Now
                };
                _context.Usuarios.Add(usuario);
            }
            else
            {
                usuario.UltimoAcceso = DateTime.Now;
                if (!string.IsNullOrEmpty(picture))
                {
                    usuario.Foto = picture;
                }
            }

            await _context.SaveChangesAsync();

            var result = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                Telefono = usuario.Telefono,
                Foto = usuario.Foto,
                Departamento = usuario.Departamento,
                Activo = usuario.Activo,
                UltimoAcceso = usuario.UltimoAcceso,
                FechaCreacion = usuario.FechaCreacion
            };

            return Ok(ApiResponse<UsuarioDto>.Ok(result, "Login exitoso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en login con Google");
            return StatusCode(500, ApiResponse<UsuarioDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Update a user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> UpdateUsuario(int id, [FromBody] UpdateUsuarioDto dto)
    {
        try
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(ApiResponse<UsuarioDto>.Fail("Usuario no encontrado"));
            }

            // Validate rol
            var validRoles = new[] { "reportante", "tecnico", "administrador" };
            if (!validRoles.Contains(dto.Rol?.ToLower()))
            {
                return BadRequest(ApiResponse<UsuarioDto>.Fail("Rol inválido"));
            }

            usuario.Nombre = dto.Nombre;
            usuario.Rol = dto.Rol ?? usuario.Rol;
            usuario.Telefono = dto.Telefono;
            usuario.Foto = dto.Foto;
            usuario.Departamento = dto.Departamento;
            usuario.Activo = dto.Activo;
            usuario.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            var result = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                Telefono = usuario.Telefono,
                Foto = usuario.Foto,
                Departamento = usuario.Departamento,
                Activo = usuario.Activo,
                UltimoAcceso = usuario.UltimoAcceso,
                FechaCreacion = usuario.FechaCreacion
            };

            return Ok(ApiResponse<UsuarioDto>.Ok(result, "Usuario actualizado correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario {Id}", id);
            return StatusCode(500, ApiResponse<UsuarioDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Toggle user active status
    /// </summary>
    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> ToggleUsuario(int id)
    {
        try
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(ApiResponse<UsuarioDto>.Fail("Usuario no encontrado"));
            }

            usuario.Activo = !usuario.Activo;
            usuario.FechaActualizacion = DateTime.Now;
            await _context.SaveChangesAsync();

            var result = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                Activo = usuario.Activo,
                FechaCreacion = usuario.FechaCreacion
            };

            return Ok(ApiResponse<UsuarioDto>.Ok(result, $"Usuario {(usuario.Activo ? "activado" : "desactivado")} correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado de usuario {Id}", id);
            return StatusCode(500, ApiResponse<UsuarioDto>.Fail("Error interno del servidor"));
        }
    }
}

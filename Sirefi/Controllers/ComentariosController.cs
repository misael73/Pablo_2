using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sirefi.Data;
using Sirefi.DTOs;
using Sirefi.Models;

namespace Sirefi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComentariosController : ControllerBase
{
    private readonly FormsDbContext _context;
    private readonly ILogger<ComentariosController> _logger;

    public ComentariosController(FormsDbContext context, ILogger<ComentariosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get comments for a report
    /// </summary>
    [HttpGet("por-reporte/{reporteId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ComentarioDto>>>> GetComentariosPorReporte(
        int reporteId, 
        [FromQuery] bool soloPublicos = true)
    {
        try
        {
            var query = _context.Comentarios
                .Include(c => c.IdUsuarioNavigation)
                .Where(c => c.IdReporte == reporteId && !c.Eliminado);

            if (soloPublicos)
            {
                query = query.Where(c => c.Publico);
            }

            var comentarios = await query
                .OrderByDescending(c => c.FechaComentario)
                .Select(c => new ComentarioDto
                {
                    Id = c.Id,
                    IdReporte = c.IdReporte,
                    IdUsuario = c.IdUsuario,
                    UsuarioNombre = c.IdUsuarioNavigation.Nombre,
                    UsuarioRol = c.IdUsuarioNavigation.Rol,
                    Comentario = c.Comentario1,
                    Tipo = c.Tipo,
                    Publico = c.Publico,
                    FechaComentario = c.FechaComentario,
                    Editado = c.Editado
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<ComentarioDto>>.Ok(comentarios, "Comentarios obtenidos correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener comentarios del reporte {ReporteId}", reporteId);
            return StatusCode(500, ApiResponse<IEnumerable<ComentarioDto>>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get a comment by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ComentarioDto>>> GetComentario(int id)
    {
        try
        {
            var comentario = await _context.Comentarios
                .Include(c => c.IdUsuarioNavigation)
                .Where(c => c.Id == id && !c.Eliminado)
                .Select(c => new ComentarioDto
                {
                    Id = c.Id,
                    IdReporte = c.IdReporte,
                    IdUsuario = c.IdUsuario,
                    UsuarioNombre = c.IdUsuarioNavigation.Nombre,
                    UsuarioRol = c.IdUsuarioNavigation.Rol,
                    Comentario = c.Comentario1,
                    Tipo = c.Tipo,
                    Publico = c.Publico,
                    FechaComentario = c.FechaComentario,
                    Editado = c.Editado
                })
                .FirstOrDefaultAsync();

            if (comentario == null)
            {
                return NotFound(ApiResponse<ComentarioDto>.Fail("Comentario no encontrado"));
            }

            return Ok(ApiResponse<ComentarioDto>.Ok(comentario, "Comentario obtenido correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener comentario {Id}", id);
            return StatusCode(500, ApiResponse<ComentarioDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Create a new comment
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ComentarioDto>>> CreateComentario([FromBody] CreateComentarioDto dto)
    {
        try
        {
            // Validate reporte exists
            var reporteExists = await _context.Reportes.AnyAsync(r => r.Id == dto.IdReporte && !r.Eliminado);
            if (!reporteExists)
            {
                return BadRequest(ApiResponse<ComentarioDto>.Fail("Reporte no encontrado"));
            }

            // Validate usuario exists
            var usuario = await _context.Usuarios.FindAsync(dto.IdUsuario);
            if (usuario == null)
            {
                return BadRequest(ApiResponse<ComentarioDto>.Fail("Usuario no encontrado"));
            }

            var comentario = new Comentario
            {
                IdReporte = dto.IdReporte,
                IdUsuario = dto.IdUsuario,
                Comentario1 = dto.Comentario,
                Tipo = dto.Tipo ?? "comentario",
                Publico = dto.Publico,
                FechaComentario = DateTime.Now,
                Editado = false,
                Eliminado = false
            };

            _context.Comentarios.Add(comentario);
            await _context.SaveChangesAsync();

            var result = new ComentarioDto
            {
                Id = comentario.Id,
                IdReporte = comentario.IdReporte,
                IdUsuario = comentario.IdUsuario,
                UsuarioNombre = usuario.Nombre,
                UsuarioRol = usuario.Rol,
                Comentario = comentario.Comentario1,
                Tipo = comentario.Tipo,
                Publico = comentario.Publico,
                FechaComentario = comentario.FechaComentario,
                Editado = comentario.Editado
            };

            return CreatedAtAction(nameof(GetComentario), new { id = comentario.Id },
                ApiResponse<ComentarioDto>.Ok(result, "Comentario creado correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear comentario");
            return StatusCode(500, ApiResponse<ComentarioDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Update a comment
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ComentarioDto>>> UpdateComentario(int id, [FromBody] UpdateComentarioDto dto)
    {
        try
        {
            var comentario = await _context.Comentarios
                .Include(c => c.IdUsuarioNavigation)
                .FirstOrDefaultAsync(c => c.Id == id && !c.Eliminado);

            if (comentario == null)
            {
                return NotFound(ApiResponse<ComentarioDto>.Fail("Comentario no encontrado"));
            }

            comentario.Comentario1 = dto.Comentario;
            comentario.Editado = true;
            comentario.FechaEdicion = DateTime.Now;

            await _context.SaveChangesAsync();

            var result = new ComentarioDto
            {
                Id = comentario.Id,
                IdReporte = comentario.IdReporte,
                IdUsuario = comentario.IdUsuario,
                UsuarioNombre = comentario.IdUsuarioNavigation.Nombre,
                UsuarioRol = comentario.IdUsuarioNavigation.Rol,
                Comentario = comentario.Comentario1,
                Tipo = comentario.Tipo,
                Publico = comentario.Publico,
                FechaComentario = comentario.FechaComentario,
                Editado = comentario.Editado
            };

            return Ok(ApiResponse<ComentarioDto>.Ok(result, "Comentario actualizado correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar comentario {Id}", id);
            return StatusCode(500, ApiResponse<ComentarioDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Delete a comment (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteComentario(int id)
    {
        try
        {
            var comentario = await _context.Comentarios.FindAsync(id);

            if (comentario == null)
            {
                return NotFound(ApiResponse.Fail("Comentario no encontrado"));
            }

            // Soft delete
            comentario.Eliminado = true;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse.Ok("Comentario eliminado correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar comentario {Id}", id);
            return StatusCode(500, ApiResponse.Fail("Error interno del servidor"));
        }
    }
}

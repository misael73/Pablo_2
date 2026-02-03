using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sirefi.Data;
using Sirefi.DTOs;
using Sirefi.Models;

namespace Sirefi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly FormsDbContext _context;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(FormsDbContext context, ILogger<CategoriasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoriaDto>>>> GetCategorias(
        [FromQuery] bool? activo = null)
    {
        try
        {
            var query = _context.Categorias.AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(c => c.Activo == activo.Value);
            }

            var categorias = await query
                .OrderBy(c => c.Nombre)
                .Select(c => new CategoriaDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    TipoDashboard = c.TipoDashboard,
                    Descripcion = c.Descripcion,
                    Icono = c.Icono,
                    Color = c.Color,
                    Activo = c.Activo,
                    FechaCreacion = c.FechaCreacion
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<CategoriaDto>>.Ok(categorias, "Categorías obtenidas correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías");
            return StatusCode(500, ApiResponse<IEnumerable<CategoriaDto>>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get a category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoriaDto>>> GetCategoria(int id)
    {
        try
        {
            var categoria = await _context.Categorias
                .Where(c => c.Id == id)
                .Select(c => new CategoriaDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    TipoDashboard = c.TipoDashboard,
                    Descripcion = c.Descripcion,
                    Icono = c.Icono,
                    Color = c.Color,
                    Activo = c.Activo,
                    FechaCreacion = c.FechaCreacion
                })
                .FirstOrDefaultAsync();

            if (categoria == null)
            {
                return NotFound(ApiResponse<CategoriaDto>.Fail("Categoría no encontrada"));
            }

            return Ok(ApiResponse<CategoriaDto>.Ok(categoria, "Categoría obtenida correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categoría {Id}", id);
            return StatusCode(500, ApiResponse<CategoriaDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoriaDto>>> CreateCategoria([FromBody] CreateCategoriaDto dto)
    {
        try
        {
            // Validate tipo_dashboard
            var validTipos = new[] { "materiales", "tics", "infraestructura", "general" };
            if (!validTipos.Contains(dto.TipoDashboard?.ToLower()))
            {
                return BadRequest(ApiResponse<CategoriaDto>.Fail("Tipo de dashboard inválido. Debe ser: materiales, tics, infraestructura o general"));
            }

            // Check if name already exists
            var exists = await _context.Categorias
                .AnyAsync(c => c.Nombre.ToLower() == dto.Nombre.ToLower());

            if (exists)
            {
                return BadRequest(ApiResponse<CategoriaDto>.Fail($"Ya existe una categoría con el nombre '{dto.Nombre}'"));
            }

            var categoria = new Categoria
            {
                Nombre = dto.Nombre,
                TipoDashboard = dto.TipoDashboard,
                Descripcion = dto.Descripcion,
                Icono = dto.Icono,
                Color = dto.Color,
                Activo = dto.Activo,
                FechaCreacion = DateTime.Now
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            var result = new CategoriaDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                TipoDashboard = categoria.TipoDashboard,
                Descripcion = categoria.Descripcion,
                Icono = categoria.Icono,
                Color = categoria.Color,
                Activo = categoria.Activo,
                FechaCreacion = categoria.FechaCreacion
            };

            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, 
                ApiResponse<CategoriaDto>.Ok(result, "Categoría creada correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear categoría");
            return StatusCode(500, ApiResponse<CategoriaDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Update a category
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CategoriaDto>>> UpdateCategoria(int id, [FromBody] UpdateCategoriaDto dto)
    {
        try
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return NotFound(ApiResponse<CategoriaDto>.Fail("Categoría no encontrada"));
            }

            // Validate tipo_dashboard
            var validTipos = new[] { "materiales", "tics", "infraestructura", "general" };
            if (!validTipos.Contains(dto.TipoDashboard?.ToLower()))
            {
                return BadRequest(ApiResponse<CategoriaDto>.Fail("Tipo de dashboard inválido"));
            }

            // Check if name already exists (excluding current record)
            var exists = await _context.Categorias
                .AnyAsync(c => c.Nombre.ToLower() == dto.Nombre.ToLower() && c.Id != id);

            if (exists)
            {
                return BadRequest(ApiResponse<CategoriaDto>.Fail($"Ya existe otra categoría con el nombre '{dto.Nombre}'"));
            }

            categoria.Nombre = dto.Nombre;
            categoria.TipoDashboard = dto.TipoDashboard;
            categoria.Descripcion = dto.Descripcion;
            categoria.Icono = dto.Icono;
            categoria.Color = dto.Color;
            categoria.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            var result = new CategoriaDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                TipoDashboard = categoria.TipoDashboard,
                Descripcion = categoria.Descripcion,
                Icono = categoria.Icono,
                Color = categoria.Color,
                Activo = categoria.Activo,
                FechaCreacion = categoria.FechaCreacion
            };

            return Ok(ApiResponse<CategoriaDto>.Ok(result, "Categoría actualizada correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar categoría {Id}", id);
            return StatusCode(500, ApiResponse<CategoriaDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Toggle category active status
    /// </summary>
    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult<ApiResponse<CategoriaDto>>> ToggleCategoria(int id)
    {
        try
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return NotFound(ApiResponse<CategoriaDto>.Fail("Categoría no encontrada"));
            }

            categoria.Activo = !categoria.Activo;
            await _context.SaveChangesAsync();

            var result = new CategoriaDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                TipoDashboard = categoria.TipoDashboard,
                Descripcion = categoria.Descripcion,
                Icono = categoria.Icono,
                Color = categoria.Color,
                Activo = categoria.Activo,
                FechaCreacion = categoria.FechaCreacion
            };

            return Ok(ApiResponse<CategoriaDto>.Ok(result, $"Categoría {(categoria.Activo ? "activada" : "desactivada")} correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado de categoría {Id}", id);
            return StatusCode(500, ApiResponse<CategoriaDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Delete a category (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteCategoria(int id)
    {
        try
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                return NotFound(ApiResponse.Fail("Categoría no encontrada"));
            }

            // Check if category has active reports
            var hasReports = await _context.Reportes
                .AnyAsync(r => r.IdCategoria == id && !r.Eliminado);

            if (hasReports)
            {
                return BadRequest(ApiResponse.Fail("No se puede eliminar la categoría porque tiene reportes activos asociados"));
            }

            // Soft delete - mark as inactive
            categoria.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.Ok("Categoría desactivada correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar categoría {Id}", id);
            return StatusCode(500, ApiResponse.Fail("Error interno del servidor"));
        }
    }
}

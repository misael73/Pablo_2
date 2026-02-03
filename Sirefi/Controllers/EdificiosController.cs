using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sirefi.Data;
using Sirefi.DTOs;
using Sirefi.Models;
using System.Text.RegularExpressions;

namespace Sirefi.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class EdificiosController : ControllerBase
{
    private readonly FormsDbContext _context;
    private readonly ILogger<EdificiosController> _logger;

    // Compiled regex for code validation (alphanumeric and hyphens)
    [GeneratedRegex(@"^[A-Z0-9\-]+$", RegexOptions.IgnoreCase)]
    private static partial Regex CodigoRegex();

    public EdificiosController(FormsDbContext context, ILogger<EdificiosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all buildings
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<EdificioDto>>>> GetEdificios(
        [FromQuery] bool? activo = null)
    {
        try
        {
            var query = _context.Edificios
                .Include(e => e.Salones)
                .AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(e => e.Activo == activo.Value);
            }

            var edificios = await query
                .OrderBy(e => e.Nombre)
                .Select(e => new EdificioDto
                {
                    Id = e.Id,
                    Codigo = e.Codigo,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    Ubicacion = e.Ubicacion,
                    Pisos = e.Pisos,
                    Activo = e.Activo,
                    FechaCreacion = e.FechaCreacion,
                    SalonesCount = e.Salones.Count(s => s.Activo)
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<EdificioDto>>.Ok(edificios, "Edificios obtenidos correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener edificios");
            return StatusCode(500, ApiResponse<IEnumerable<EdificioDto>>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get a building by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<EdificioDto>>> GetEdificio(int id)
    {
        try
        {
            var edificio = await _context.Edificios
                .Include(e => e.Salones)
                .Where(e => e.Id == id)
                .Select(e => new EdificioDto
                {
                    Id = e.Id,
                    Codigo = e.Codigo,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    Ubicacion = e.Ubicacion,
                    Pisos = e.Pisos,
                    Activo = e.Activo,
                    FechaCreacion = e.FechaCreacion,
                    SalonesCount = e.Salones.Count(s => s.Activo)
                })
                .FirstOrDefaultAsync();

            if (edificio == null)
            {
                return NotFound(ApiResponse<EdificioDto>.Fail("Edificio no encontrado"));
            }

            return Ok(ApiResponse<EdificioDto>.Ok(edificio, "Edificio obtenido correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener edificio {Id}", id);
            return StatusCode(500, ApiResponse<EdificioDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Create a new building
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<EdificioDto>>> CreateEdificio([FromBody] CreateEdificioDto dto)
    {
        try
        {
            // Validate codigo format
            if (string.IsNullOrWhiteSpace(dto.Codigo) || dto.Codigo.Length < 2)
            {
                return BadRequest(ApiResponse<EdificioDto>.Fail("El código debe tener al menos 2 caracteres"));
            }

            if (!CodigoRegex().IsMatch(dto.Codigo))
            {
                return BadRequest(ApiResponse<EdificioDto>.Fail("El código solo puede contener letras, números y guiones"));
            }

            // Check if name already exists
            var nameExists = await _context.Edificios
                .AnyAsync(e => e.Nombre.ToLower() == dto.Nombre.ToLower());

            if (nameExists)
            {
                return BadRequest(ApiResponse<EdificioDto>.Fail($"Ya existe un edificio con el nombre '{dto.Nombre}'"));
            }

            // Check if codigo already exists
            var codigoExists = await _context.Edificios
                .AnyAsync(e => e.Codigo != null && e.Codigo.ToLower() == dto.Codigo.ToLower());

            if (codigoExists)
            {
                return BadRequest(ApiResponse<EdificioDto>.Fail($"Ya existe un edificio con el código '{dto.Codigo}'"));
            }

            var edificio = new Edificio
            {
                Codigo = dto.Codigo,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Ubicacion = dto.Ubicacion,
                Pisos = dto.Pisos,
                Activo = dto.Activo,
                FechaCreacion = DateTime.Now
            };

            _context.Edificios.Add(edificio);
            await _context.SaveChangesAsync();

            var result = new EdificioDto
            {
                Id = edificio.Id,
                Codigo = edificio.Codigo,
                Nombre = edificio.Nombre,
                Descripcion = edificio.Descripcion,
                Ubicacion = edificio.Ubicacion,
                Pisos = edificio.Pisos,
                Activo = edificio.Activo,
                FechaCreacion = edificio.FechaCreacion,
                SalonesCount = 0
            };

            return CreatedAtAction(nameof(GetEdificio), new { id = edificio.Id },
                ApiResponse<EdificioDto>.Ok(result, "Edificio creado correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear edificio");
            return StatusCode(500, ApiResponse<EdificioDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Update a building
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<EdificioDto>>> UpdateEdificio(int id, [FromBody] UpdateEdificioDto dto)
    {
        try
        {
            var edificio = await _context.Edificios
                .Include(e => e.Salones)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (edificio == null)
            {
                return NotFound(ApiResponse<EdificioDto>.Fail("Edificio no encontrado"));
            }

            // Check if name already exists (excluding current record)
            var nameExists = await _context.Edificios
                .AnyAsync(e => e.Nombre.ToLower() == dto.Nombre.ToLower() && e.Id != id);

            if (nameExists)
            {
                return BadRequest(ApiResponse<EdificioDto>.Fail($"Ya existe otro edificio con el nombre '{dto.Nombre}'"));
            }

            // Check if codigo already exists (excluding current record)
            var codigoExists = await _context.Edificios
                .AnyAsync(e => e.Codigo != null && e.Codigo.ToLower() == dto.Codigo.ToLower() && e.Id != id);

            if (codigoExists)
            {
                return BadRequest(ApiResponse<EdificioDto>.Fail($"Ya existe otro edificio con el código '{dto.Codigo}'"));
            }

            // If deactivating, check for active salones
            if (!dto.Activo && edificio.Salones.Any(s => s.Activo))
            {
                return BadRequest(ApiResponse<EdificioDto>.Fail("No se puede desactivar un edificio con salones activos"));
            }

            edificio.Codigo = dto.Codigo;
            edificio.Nombre = dto.Nombre;
            edificio.Descripcion = dto.Descripcion;
            edificio.Ubicacion = dto.Ubicacion;
            edificio.Pisos = dto.Pisos;
            edificio.Activo = dto.Activo;
            edificio.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            var result = new EdificioDto
            {
                Id = edificio.Id,
                Codigo = edificio.Codigo,
                Nombre = edificio.Nombre,
                Descripcion = edificio.Descripcion,
                Ubicacion = edificio.Ubicacion,
                Pisos = edificio.Pisos,
                Activo = edificio.Activo,
                FechaCreacion = edificio.FechaCreacion,
                SalonesCount = edificio.Salones.Count(s => s.Activo)
            };

            return Ok(ApiResponse<EdificioDto>.Ok(result, "Edificio actualizado correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar edificio {Id}", id);
            return StatusCode(500, ApiResponse<EdificioDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Delete a building
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteEdificio(int id)
    {
        try
        {
            var edificio = await _context.Edificios
                .Include(e => e.Salones)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (edificio == null)
            {
                return NotFound(ApiResponse.Fail("Edificio no encontrado"));
            }

            // Check for associated salones
            if (edificio.Salones.Any())
            {
                return BadRequest(ApiResponse.Fail("No se puede eliminar un edificio que tiene salones asociados"));
            }

            _context.Edificios.Remove(edificio);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.Ok("Edificio eliminado correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar edificio {Id}", id);
            return StatusCode(500, ApiResponse.Fail("Error interno del servidor"));
        }
    }
}

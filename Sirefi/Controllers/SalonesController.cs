using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sirefi.Data;
using Sirefi.DTOs;
using Sirefi.Models;

namespace Sirefi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalonesController : ControllerBase
{
    private readonly FormsDbContext _context;
    private readonly ILogger<SalonesController> _logger;

    public SalonesController(FormsDbContext context, ILogger<SalonesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all rooms, optionally filtered by building
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<SalonDto>>>> GetSalones(
        [FromQuery] int? edificioId = null,
        [FromQuery] bool? activo = null)
    {
        try
        {
            var query = _context.Salones
                .Include(s => s.IdEdificioNavigation)
                .AsQueryable();

            if (edificioId.HasValue)
            {
                query = query.Where(s => s.IdEdificio == edificioId.Value);
            }

            if (activo.HasValue)
            {
                query = query.Where(s => s.Activo == activo.Value);
            }

            var salones = await query
                .OrderBy(s => s.IdEdificioNavigation.Nombre)
                .ThenBy(s => s.Nombre)
                .Select(s => new SalonDto
                {
                    Id = s.Id,
                    IdEdificio = s.IdEdificio,
                    EdificioNombre = s.IdEdificioNavigation.Nombre,
                    Codigo = s.Codigo,
                    Nombre = s.Nombre,
                    TipoEspacio = s.TipoEspacio,
                    Capacidad = s.Capacidad,
                    Piso = s.Piso,
                    Descripcion = s.Descripcion,
                    Activo = s.Activo,
                    FechaCreacion = s.FechaCreacion
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<SalonDto>>.Ok(salones, "Salones obtenidos correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener salones");
            return StatusCode(500, ApiResponse<IEnumerable<SalonDto>>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get rooms by building ID (for dropdown in forms)
    /// </summary>
    [HttpGet("por-edificio/{edificioId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SalonDto>>>> GetSalonesPorEdificio(int edificioId)
    {
        try
        {
            var salones = await _context.Salones
                .Where(s => s.IdEdificio == edificioId && s.Activo)
                .OrderBy(s => s.Nombre)
                .Select(s => new SalonDto
                {
                    Id = s.Id,
                    IdEdificio = s.IdEdificio,
                    Nombre = s.Nombre,
                    TipoEspacio = s.TipoEspacio,
                    Piso = s.Piso,
                    Activo = s.Activo,
                    FechaCreacion = s.FechaCreacion
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<SalonDto>>.Ok(salones, "Salones obtenidos correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener salones por edificio {EdificioId}", edificioId);
            return StatusCode(500, ApiResponse<IEnumerable<SalonDto>>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get a room by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SalonDto>>> GetSalon(int id)
    {
        try
        {
            var salon = await _context.Salones
                .Include(s => s.IdEdificioNavigation)
                .Where(s => s.Id == id)
                .Select(s => new SalonDto
                {
                    Id = s.Id,
                    IdEdificio = s.IdEdificio,
                    EdificioNombre = s.IdEdificioNavigation.Nombre,
                    Codigo = s.Codigo,
                    Nombre = s.Nombre,
                    TipoEspacio = s.TipoEspacio,
                    Capacidad = s.Capacidad,
                    Piso = s.Piso,
                    Descripcion = s.Descripcion,
                    Activo = s.Activo,
                    FechaCreacion = s.FechaCreacion
                })
                .FirstOrDefaultAsync();

            if (salon == null)
            {
                return NotFound(ApiResponse<SalonDto>.Fail("Salón no encontrado"));
            }

            return Ok(ApiResponse<SalonDto>.Ok(salon, "Salón obtenido correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener salón {Id}", id);
            return StatusCode(500, ApiResponse<SalonDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Create a new room
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<SalonDto>>> CreateSalon([FromBody] CreateSalonDto dto)
    {
        try
        {
            // Verify edificio exists
            var edificioExists = await _context.Edificios.AnyAsync(e => e.Id == dto.IdEdificio);
            if (!edificioExists)
            {
                return BadRequest(ApiResponse<SalonDto>.Fail("El edificio especificado no existe"));
            }

            // Check if name already exists in this edificio
            var exists = await _context.Salones
                .AnyAsync(s => s.Nombre.ToLower() == dto.Nombre.ToLower() && s.IdEdificio == dto.IdEdificio);

            if (exists)
            {
                return BadRequest(ApiResponse<SalonDto>.Fail($"Ya existe un salón con el nombre '{dto.Nombre}' en este edificio"));
            }

            var salon = new Salone
            {
                IdEdificio = dto.IdEdificio,
                Codigo = dto.Codigo,
                Nombre = dto.Nombre,
                TipoEspacio = dto.TipoEspacio,
                Capacidad = dto.Capacidad,
                Piso = dto.Piso,
                Descripcion = dto.Descripcion,
                Activo = dto.Activo,
                FechaCreacion = DateTime.Now
            };

            _context.Salones.Add(salon);
            await _context.SaveChangesAsync();

            var edificio = await _context.Edificios.FindAsync(dto.IdEdificio);

            var result = new SalonDto
            {
                Id = salon.Id,
                IdEdificio = salon.IdEdificio,
                EdificioNombre = edificio?.Nombre,
                Codigo = salon.Codigo,
                Nombre = salon.Nombre,
                TipoEspacio = salon.TipoEspacio,
                Capacidad = salon.Capacidad,
                Piso = salon.Piso,
                Descripcion = salon.Descripcion,
                Activo = salon.Activo,
                FechaCreacion = salon.FechaCreacion
            };

            return CreatedAtAction(nameof(GetSalon), new { id = salon.Id },
                ApiResponse<SalonDto>.Ok(result, "Salón creado correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear salón");
            return StatusCode(500, ApiResponse<SalonDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Update a room
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<SalonDto>>> UpdateSalon(int id, [FromBody] UpdateSalonDto dto)
    {
        try
        {
            var salon = await _context.Salones.FindAsync(id);

            if (salon == null)
            {
                return NotFound(ApiResponse<SalonDto>.Fail("Salón no encontrado"));
            }

            // Verify edificio exists
            var edificioExists = await _context.Edificios.AnyAsync(e => e.Id == dto.IdEdificio);
            if (!edificioExists)
            {
                return BadRequest(ApiResponse<SalonDto>.Fail("El edificio especificado no existe"));
            }

            // Check if name already exists in this edificio (excluding current record)
            var exists = await _context.Salones
                .AnyAsync(s => s.Nombre.ToLower() == dto.Nombre.ToLower() && s.IdEdificio == dto.IdEdificio && s.Id != id);

            if (exists)
            {
                return BadRequest(ApiResponse<SalonDto>.Fail($"Ya existe otro salón con el nombre '{dto.Nombre}' en este edificio"));
            }

            salon.IdEdificio = dto.IdEdificio;
            salon.Codigo = dto.Codigo;
            salon.Nombre = dto.Nombre;
            salon.TipoEspacio = dto.TipoEspacio;
            salon.Capacidad = dto.Capacidad;
            salon.Piso = dto.Piso;
            salon.Descripcion = dto.Descripcion;
            salon.Activo = dto.Activo;
            salon.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            var edificio = await _context.Edificios.FindAsync(dto.IdEdificio);

            var result = new SalonDto
            {
                Id = salon.Id,
                IdEdificio = salon.IdEdificio,
                EdificioNombre = edificio?.Nombre,
                Codigo = salon.Codigo,
                Nombre = salon.Nombre,
                TipoEspacio = salon.TipoEspacio,
                Capacidad = salon.Capacidad,
                Piso = salon.Piso,
                Descripcion = salon.Descripcion,
                Activo = salon.Activo,
                FechaCreacion = salon.FechaCreacion
            };

            return Ok(ApiResponse<SalonDto>.Ok(result, "Salón actualizado correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar salón {Id}", id);
            return StatusCode(500, ApiResponse<SalonDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Delete a room
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteSalon(int id)
    {
        try
        {
            var salon = await _context.Salones.FindAsync(id);

            if (salon == null)
            {
                return NotFound(ApiResponse.Fail("Salón no encontrado"));
            }

            // Check for associated reports
            var hasReports = await _context.Reportes.AnyAsync(r => r.IdSalon == id && !r.Eliminado);
            if (hasReports)
            {
                return BadRequest(ApiResponse.Fail("No se puede eliminar un salón que tiene reportes asociados"));
            }

            _context.Salones.Remove(salon);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.Ok("Salón eliminado correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar salón {Id}", id);
            return StatusCode(500, ApiResponse.Fail("Error interno del servidor"));
        }
    }
}

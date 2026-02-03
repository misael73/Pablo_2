using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sirefi.Data;
using Sirefi.DTOs;

namespace Sirefi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstadosController : ControllerBase
{
    private readonly FormsDbContext _context;
    private readonly ILogger<EstadosController> _logger;

    public EstadosController(FormsDbContext context, ILogger<EstadosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all states
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<EstadoDto>>>> GetEstados([FromQuery] bool? activo = null)
    {
        try
        {
            var query = _context.Estados.AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(e => e.Activo == activo.Value);
            }

            var estados = await query
                .OrderBy(e => e.Orden)
                .Select(e => new EstadoDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Orden = e.Orden,
                    EsFinal = e.EsFinal,
                    Color = e.Color,
                    Descripcion = e.Descripcion,
                    Activo = e.Activo
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<EstadoDto>>.Ok(estados, "Estados obtenidos correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estados");
            return StatusCode(500, ApiResponse<IEnumerable<EstadoDto>>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get a state by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<EstadoDto>>> GetEstado(int id)
    {
        try
        {
            var estado = await _context.Estados
                .Where(e => e.Id == id)
                .Select(e => new EstadoDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Orden = e.Orden,
                    EsFinal = e.EsFinal,
                    Color = e.Color,
                    Descripcion = e.Descripcion,
                    Activo = e.Activo
                })
                .FirstOrDefaultAsync();

            if (estado == null)
            {
                return NotFound(ApiResponse<EstadoDto>.Fail("Estado no encontrado"));
            }

            return Ok(ApiResponse<EstadoDto>.Ok(estado, "Estado obtenido correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estado {Id}", id);
            return StatusCode(500, ApiResponse<EstadoDto>.Fail("Error interno del servidor"));
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class PrioridadesController : ControllerBase
{
    private readonly FormsDbContext _context;
    private readonly ILogger<PrioridadesController> _logger;

    public PrioridadesController(FormsDbContext context, ILogger<PrioridadesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all priorities
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PrioridadDto>>>> GetPrioridades([FromQuery] bool? activo = null)
    {
        try
        {
            var query = _context.Prioridades.AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(p => p.Activo == activo.Value);
            }

            var prioridades = await query
                .OrderBy(p => p.Nivel)
                .Select(p => new PrioridadDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Nivel = p.Nivel,
                    Color = p.Color,
                    Descripcion = p.Descripcion,
                    Activo = p.Activo
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<PrioridadDto>>.Ok(prioridades, "Prioridades obtenidas correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prioridades");
            return StatusCode(500, ApiResponse<IEnumerable<PrioridadDto>>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get a priority by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PrioridadDto>>> GetPrioridad(int id)
    {
        try
        {
            var prioridad = await _context.Prioridades
                .Where(p => p.Id == id)
                .Select(p => new PrioridadDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Nivel = p.Nivel,
                    Color = p.Color,
                    Descripcion = p.Descripcion,
                    Activo = p.Activo
                })
                .FirstOrDefaultAsync();

            if (prioridad == null)
            {
                return NotFound(ApiResponse<PrioridadDto>.Fail("Prioridad no encontrada"));
            }

            return Ok(ApiResponse<PrioridadDto>.Ok(prioridad, "Prioridad obtenida correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener prioridad {Id}", id);
            return StatusCode(500, ApiResponse<PrioridadDto>.Fail("Error interno del servidor"));
        }
    }
}

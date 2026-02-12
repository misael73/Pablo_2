using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sirefi.Data;
using Sirefi.DTOs;
using Sirefi.Models;

namespace Sirefi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly FormsDbContext _context;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(FormsDbContext context, ILogger<ReportesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all reports with optional filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReporteDto>>>> GetReportes(
        [FromQuery] int? usuarioId = null,
        [FromQuery] string? estado = null,
        [FromQuery] string? tipoDashboard = null,
        [FromQuery] int? categoriaId = null,
        [FromQuery] int? edificioId = null)
    {
        try
        {
            var query = _context.VwReportesCompletos.AsQueryable();

            // Filter by usuario (reportante)
            if (usuarioId.HasValue)
            {
                var reporteIds = await _context.Reportes
                    .Where(r => r.IdReportante == usuarioId.Value && !r.Eliminado)
                    .Select(r => r.Id)
                    .ToListAsync();
                
                query = query.Where(r => reporteIds.Contains(r.Id));
            }

            // Filter by estado
            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(r => r.Estado != null && r.Estado.ToLower() == estado.ToLower());
            }

            // Filter by tipo_dashboard (categoría)
            if (!string.IsNullOrWhiteSpace(tipoDashboard))
            {
                var categoriaIds = await _context.Categorias
                    .Where(c => c.TipoDashboard != null && c.TipoDashboard.ToLower() == tipoDashboard.ToLower())
                    .Select(c => c.Id)
                    .ToListAsync();

                var reporteIds = await _context.Reportes
                    .Where(r => categoriaIds.Contains(r.IdCategoria) && !r.Eliminado)
                    .Select(r => r.Id)
                    .ToListAsync();

                query = query.Where(r => reporteIds.Contains(r.Id));
            }

            // Filter by categoria
            if (categoriaId.HasValue)
            {
                var reporteIds = await _context.Reportes
                    .Where(r => r.IdCategoria == categoriaId.Value && !r.Eliminado)
                    .Select(r => r.Id)
                    .ToListAsync();

                query = query.Where(r => reporteIds.Contains(r.Id));
            }

            // Filter by edificio
            if (edificioId.HasValue)
            {
                var reporteIds = await _context.Reportes
                    .Where(r => r.IdEdificio == edificioId.Value && !r.Eliminado)
                    .Select(r => r.Id)
                    .ToListAsync();

                query = query.Where(r => reporteIds.Contains(r.Id));
            }

            // Only show non-deleted reports
            query = query.Where(r => !r.Eliminado);

            var reportes = await query
                .OrderByDescending(r => r.FechaReporte)
                .Select(r => new ReporteDto
                {
                    Id = r.Id,
                    Folio = r.Folio ?? "",
                    IdEdificio = null,
                    EdificioNombre = r.Edificio,
                    IdSalon = null,
                    SalonNombre = r.Salon,
                    UbicacionAdicional = r.UbicacionAdicional,
                    IdCategoria = 0,
                    CategoriaNombre = r.Categoria,
                    Titulo = r.Titulo,
                    Descripcion = r.Descripcion ?? "",
                    IdPrioridad = 0,
                    PrioridadNombre = r.Prioridad,
                    PrioridadColor = r.PrioridadColor,
                    IdEstado = 0,
                    EstadoNombre = r.Estado,
                    EstadoColor = r.EstadoColor,
                    IdReportante = 0,
                    ReportanteNombre = r.ReportanteNombre,
                    TecnicoNombre = r.TecnicoNombre,
                    FechaReporte = r.FechaReporte,
                    FechaAsignacion = r.FechaAsignacion
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<ReporteDto>>.Ok(reportes, "Reportes obtenidos correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reportes");
            return StatusCode(500, ApiResponse<IEnumerable<ReporteDto>>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get a report by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ReporteDto>>> GetReporte(int id)
    {
        try
        {
            var reporte = await _context.Reportes
                .Include(r => r.IdEdificioNavigation)
                .Include(r => r.IdSalonNavigation)
                .Include(r => r.IdCategoriaNavigation)
                .Include(r => r.IdPrioridadNavigation)
                .Include(r => r.IdEstadoNavigation)
                .Include(r => r.IdReportanteNavigation)
                .Include(r => r.IdAsignadoANavigation)
                .Where(r => r.Id == id && !r.Eliminado)
                .FirstOrDefaultAsync();

            if (reporte == null)
            {
                return NotFound(ApiResponse<ReporteDto>.Fail("Reporte no encontrado"));
            }

            // Get last public comment
            var ultimaAccion = await _context.Comentarios
                .Where(c => c.IdReporte == id && c.Publico && !c.Eliminado)
                .OrderByDescending(c => c.FechaComentario)
                .Select(c => c.Comentario1)
                .FirstOrDefaultAsync();

            var result = new ReporteDto
            {
                Id = reporte.Id,
                Folio = reporte.Folio,
                IdEdificio = reporte.IdEdificio,
                EdificioNombre = reporte.IdEdificioNavigation?.Nombre,
                IdSalon = reporte.IdSalon,
                SalonNombre = reporte.IdSalonNavigation?.Nombre,
                UbicacionAdicional = reporte.UbicacionAdicional,
                IdCategoria = reporte.IdCategoria,
                CategoriaNombre = reporte.IdCategoriaNavigation.Nombre,
                Subcategoria = reporte.Subcategoria,
                Titulo = reporte.Titulo,
                Descripcion = reporte.Descripcion,
                IdPrioridad = reporte.IdPrioridad,
                PrioridadNombre = reporte.IdPrioridadNavigation.Nombre,
                PrioridadColor = reporte.IdPrioridadNavigation.Color,
                IdEstado = reporte.IdEstado,
                EstadoNombre = reporte.IdEstadoNavigation.Nombre,
                EstadoColor = reporte.IdEstadoNavigation.Color,
                IdReportante = reporte.IdReportante,
                ReportanteNombre = reporte.IdReportanteNavigation.Nombre,
                ReportanteCorreo = reporte.IdReportanteNavigation.Correo,
                ReportanteDepartamento = reporte.IdReportanteNavigation.Departamento,
                IdAsignadoA = reporte.IdAsignadoA,
                TecnicoNombre = reporte.IdAsignadoANavigation?.Nombre,
                FechaReporte = reporte.FechaReporte,
                FechaAsignacion = reporte.FechaAsignacion,
                FechaFinalizacion = reporte.FechaFinalizacion,
                UltimaAccion = ultimaAccion
            };

            return Ok(ApiResponse<ReporteDto>.Ok(result, "Reporte obtenido correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reporte {Id}", id);
            return StatusCode(500, ApiResponse<ReporteDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get reports by user (my reports)
    /// </summary>
    [HttpGet("mis-reportes/{usuarioId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReporteDto>>>> GetMisReportes(int usuarioId)
    {
        try
        {
            var reportes = await _context.Reportes
                .Include(r => r.IdEdificioNavigation)
                .Include(r => r.IdSalonNavigation)
                .Include(r => r.IdCategoriaNavigation)
                .Include(r => r.IdPrioridadNavigation)
                .Include(r => r.IdEstadoNavigation)
                .Where(r => r.IdReportante == usuarioId && !r.Eliminado)
                .OrderByDescending(r => r.FechaReporte)
                .Select(r => new ReporteDto
                {
                    Id = r.Id,
                    Folio = r.Folio,
                    IdEdificio = r.IdEdificio,
                    EdificioNombre = r.IdEdificioNavigation != null ? r.IdEdificioNavigation.Nombre : null,
                    IdSalon = r.IdSalon,
                    SalonNombre = r.IdSalonNavigation != null ? r.IdSalonNavigation.Nombre : null,
                    IdCategoria = r.IdCategoria,
                    CategoriaNombre = r.IdCategoriaNavigation.Nombre,
                    Titulo = r.Titulo,
                    Descripcion = r.Descripcion,
                    IdPrioridad = r.IdPrioridad,
                    PrioridadNombre = r.IdPrioridadNavigation.Nombre,
                    PrioridadColor = r.IdPrioridadNavigation.Color,
                    IdEstado = r.IdEstado,
                    EstadoNombre = r.IdEstadoNavigation.Nombre,
                    EstadoColor = r.IdEstadoNavigation.Color,
                    IdReportante = r.IdReportante,
                    FechaReporte = r.FechaReporte,
                    FechaAsignacion = r.FechaAsignacion,
                    FechaFinalizacion = r.FechaFinalizacion
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<ReporteDto>>.Ok(reportes, "Reportes obtenidos correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reportes del usuario {UsuarioId}", usuarioId);
            return StatusCode(500, ApiResponse<IEnumerable<ReporteDto>>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get reports by dashboard type (tics, materiales, infraestructura)
    /// </summary>
    [HttpGet("por-dashboard/{tipoDashboard}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReporteDto>>>> GetReportesByDashboard(string tipoDashboard)
    {
        try
        {
            // Get category IDs that match the dashboard type
            var dashboardTypes = tipoDashboard.ToLower() == "materiales" 
                ? new[] { "materiales", "infraestructura" }
                : new[] { tipoDashboard.ToLower() };

            var categoriaIds = await _context.Categorias
                .Where(c => c.TipoDashboard != null && dashboardTypes.Contains(c.TipoDashboard.ToLower()))
                .Select(c => c.Id)
                .ToListAsync();

            var reportes = await _context.Reportes
                .Include(r => r.IdEdificioNavigation)
                .Include(r => r.IdSalonNavigation)
                .Include(r => r.IdCategoriaNavigation)
                .Include(r => r.IdPrioridadNavigation)
                .Include(r => r.IdEstadoNavigation)
                .Include(r => r.IdReportanteNavigation)
                .Where(r => categoriaIds.Contains(r.IdCategoria) && !r.Eliminado)
                .OrderByDescending(r => r.FechaReporte)
                .Select(r => new ReporteDto
                {
                    Id = r.Id,
                    Folio = r.Folio,
                    IdEdificio = r.IdEdificio,
                    EdificioNombre = r.IdEdificioNavigation != null ? r.IdEdificioNavigation.Nombre : null,
                    IdSalon = r.IdSalon,
                    SalonNombre = r.IdSalonNavigation != null ? r.IdSalonNavigation.Nombre : null,
                    IdCategoria = r.IdCategoria,
                    CategoriaNombre = r.IdCategoriaNavigation.Nombre,
                    Titulo = r.Titulo,
                    Descripcion = r.Descripcion,
                    IdPrioridad = r.IdPrioridad,
                    PrioridadNombre = r.IdPrioridadNavigation.Nombre,
                    PrioridadColor = r.IdPrioridadNavigation.Color,
                    IdEstado = r.IdEstado,
                    EstadoNombre = r.IdEstadoNavigation.Nombre,
                    EstadoColor = r.IdEstadoNavigation.Color,
                    IdReportante = r.IdReportante,
                    ReportanteNombre = r.IdReportanteNavigation.Nombre,
                    FechaReporte = r.FechaReporte,
                    FechaAsignacion = r.FechaAsignacion,
                    FechaFinalizacion = r.FechaFinalizacion
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<ReporteDto>>.Ok(reportes, "Reportes obtenidos correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reportes por dashboard {TipoDashboard}", tipoDashboard);
            return StatusCode(500, ApiResponse<IEnumerable<ReporteDto>>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Create a new report
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReporteDto>>> CreateReporte([FromBody] CreateReporteDto dto)
    {
        try
        {
            // Validate categoria exists
            var categoria = await _context.Categorias.FindAsync(dto.IdCategoria);
            if (categoria == null)
            {
                return BadRequest(ApiResponse<ReporteDto>.Fail("Categoría no encontrada"));
            }

            // Validate edificio exists
            var edificio = await _context.Edificios.FindAsync(dto.IdEdificio);
            if (edificio == null)
            {
                return BadRequest(ApiResponse<ReporteDto>.Fail("Edificio no encontrado"));
            }

            // Validate reportante exists
            var reportante = await _context.Usuarios.FindAsync(dto.IdReportante);
            if (reportante == null)
            {
                return BadRequest(ApiResponse<ReporteDto>.Fail("Usuario reportante no encontrado"));
            }

            // Get default prioridad (Media)
            var prioridad = await _context.Prioridades.FirstOrDefaultAsync(p => p.Nombre == "Media" && p.Activo);
            if (prioridad == null)
            {
                prioridad = await _context.Prioridades.FirstOrDefaultAsync(p => p.Activo);
            }
            if (prioridad == null)
            {
                return BadRequest(ApiResponse<ReporteDto>.Fail("No hay prioridades configuradas en el sistema"));
            }

            // Get default estado (Recibido)
            var estado = await _context.Estados.FirstOrDefaultAsync(e => e.Nombre == "Recibido" && e.Activo);
            if (estado == null)
            {
                estado = await _context.Estados.FirstOrDefaultAsync(e => e.Activo);
            }
            if (estado == null)
            {
                return BadRequest(ApiResponse<ReporteDto>.Fail("No hay estados configurados en el sistema"));
            }

            // Generate unique folio using timestamp + GUID segment for uniqueness
            var folio = $"REP-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

            // Generate titulo from description (safe slicing)
            var maxLen = Math.Min(200, dto.Descripcion.Length);
            var titulo = dto.Titulo ?? dto.Descripcion[..maxLen];

            var reporte = new Reporte
            {
                Folio = folio,
                IdEdificio = dto.IdEdificio,
                IdSalon = dto.IdSalon,
                UbicacionAdicional = dto.UbicacionAdicional,
                IdCategoria = dto.IdCategoria,
                Subcategoria = dto.Subcategoria,
                Titulo = titulo,
                Descripcion = dto.Descripcion,
                IdPrioridad = prioridad.Id,
                IdEstado = estado.Id,
                IdReportante = dto.IdReportante,
                FechaReporte = DateTime.Now,
                Eliminado = false
            };

            _context.Reportes.Add(reporte);
            await _context.SaveChangesAsync();

            var result = new ReporteDto
            {
                Id = reporte.Id,
                Folio = reporte.Folio,
                IdEdificio = reporte.IdEdificio,
                EdificioNombre = edificio.Nombre,
                IdSalon = reporte.IdSalon,
                IdCategoria = reporte.IdCategoria,
                CategoriaNombre = categoria.Nombre,
                Titulo = reporte.Titulo,
                Descripcion = reporte.Descripcion,
                IdPrioridad = reporte.IdPrioridad,
                PrioridadNombre = prioridad.Nombre,
                IdEstado = reporte.IdEstado,
                EstadoNombre = estado.Nombre,
                IdReportante = reporte.IdReportante,
                ReportanteNombre = reportante.Nombre,
                FechaReporte = reporte.FechaReporte
            };

            return CreatedAtAction(nameof(GetReporte), new { id = reporte.Id },
                ApiResponse<ReporteDto>.Ok(result, $"Reporte creado correctamente con folio: {folio}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear reporte");
            return StatusCode(500, ApiResponse<ReporteDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Update report status (for admins/technicians)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ReporteDto>>> UpdateReporte(int id, [FromBody] UpdateReporteDto dto)
    {
        try
        {
            var reporte = await _context.Reportes
                .Include(r => r.IdEdificioNavigation)
                .Include(r => r.IdSalonNavigation)
                .Include(r => r.IdCategoriaNavigation)
                .Include(r => r.IdReportanteNavigation)
                .FirstOrDefaultAsync(r => r.Id == id && !r.Eliminado);

            if (reporte == null)
            {
                return NotFound(ApiResponse<ReporteDto>.Fail("Reporte no encontrado"));
            }

            // Get old state for history
            var oldEstadoId = reporte.IdEstado;

            // Validate new estado
            var estado = await _context.Estados.FindAsync(dto.IdEstado);
            if (estado == null)
            {
                return BadRequest(ApiResponse<ReporteDto>.Fail("Estado no encontrado"));
            }

            // Validate new prioridad
            var prioridad = await _context.Prioridades.FindAsync(dto.IdPrioridad);
            if (prioridad == null)
            {
                return BadRequest(ApiResponse<ReporteDto>.Fail("Prioridad no encontrada"));
            }

            // Update report
            reporte.IdEstado = dto.IdEstado;
            reporte.IdPrioridad = dto.IdPrioridad;
            reporte.IdAsignadoA = dto.IdAsignadoA;
            reporte.ActualizadoPor = dto.ActualizadoPor;
            reporte.FechaActualizacion = DateTime.Now;

            // Update assignment date if changing to "En Proceso"
            if (estado.Nombre == "En Proceso" && !reporte.FechaAsignacion.HasValue)
            {
                reporte.FechaAsignacion = DateTime.Now;
            }

            // Update finalization date if resolved
            if (estado.Nombre == "Resuelto")
            {
                reporte.FechaFinalizacion = DateTime.Now;
            }

            // Add comment if provided
            if (!string.IsNullOrWhiteSpace(dto.Comentario))
            {
                var comentario = new Comentario
                {
                    IdReporte = id,
                    IdUsuario = dto.ActualizadoPor,
                    Comentario1 = dto.Comentario,
                    Tipo = "accion",
                    Publico = true,
                    FechaComentario = DateTime.Now
                };
                _context.Comentarios.Add(comentario);
            }

            // Record state change in history
            var historial = new HistorialEstado
            {
                IdReporte = id,
                IdEstadoAnterior = oldEstadoId,
                IdEstadoNuevo = dto.IdEstado,
                IdUsuario = dto.ActualizadoPor,
                FechaCambio = DateTime.Now
            };
            _context.HistorialEstados.Add(historial);

            await _context.SaveChangesAsync();

            var tecnico = dto.IdAsignadoA.HasValue 
                ? await _context.Usuarios.FindAsync(dto.IdAsignadoA.Value) 
                : null;

            var result = new ReporteDto
            {
                Id = reporte.Id,
                Folio = reporte.Folio,
                IdEdificio = reporte.IdEdificio,
                EdificioNombre = reporte.IdEdificioNavigation?.Nombre,
                IdSalon = reporte.IdSalon,
                SalonNombre = reporte.IdSalonNavigation?.Nombre,
                IdCategoria = reporte.IdCategoria,
                CategoriaNombre = reporte.IdCategoriaNavigation.Nombre,
                Titulo = reporte.Titulo,
                Descripcion = reporte.Descripcion,
                IdPrioridad = reporte.IdPrioridad,
                PrioridadNombre = prioridad.Nombre,
                PrioridadColor = prioridad.Color,
                IdEstado = reporte.IdEstado,
                EstadoNombre = estado.Nombre,
                EstadoColor = estado.Color,
                IdReportante = reporte.IdReportante,
                ReportanteNombre = reporte.IdReportanteNavigation.Nombre,
                IdAsignadoA = reporte.IdAsignadoA,
                TecnicoNombre = tecnico?.Nombre,
                FechaReporte = reporte.FechaReporte,
                FechaAsignacion = reporte.FechaAsignacion,
                FechaFinalizacion = reporte.FechaFinalizacion
            };

            return Ok(ApiResponse<ReporteDto>.Ok(result, "Reporte actualizado correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar reporte {Id}", id);
            return StatusCode(500, ApiResponse<ReporteDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Delete a report (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteReporte(int id)
    {
        try
        {
            var reporte = await _context.Reportes.FindAsync(id);

            if (reporte == null)
            {
                return NotFound(ApiResponse.Fail("Reporte no encontrado"));
            }

            // Soft delete
            reporte.Eliminado = true;
            reporte.FechaEliminacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse.Ok("Reporte eliminado correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar reporte {Id}", id);
            return StatusCode(500, ApiResponse.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get report statistics
    /// </summary>
    [HttpGet("estadisticas")]
    public async Task<ActionResult<ApiResponse<ReporteStatsDto>>> GetEstadisticas(
        [FromQuery] int? usuarioId = null,
        [FromQuery] string? tipoDashboard = null)
    {
        try
        {
            var query = _context.Reportes
                .Include(r => r.IdEstadoNavigation)
                .Include(r => r.IdCategoriaNavigation)
                .Where(r => !r.Eliminado);

            // Filter by user if provided
            if (usuarioId.HasValue)
            {
                query = query.Where(r => r.IdReportante == usuarioId.Value);
            }

            // Filter by tipoDashboard if provided
            if (!string.IsNullOrWhiteSpace(tipoDashboard))
            {
                query = query.Where(r => r.IdCategoriaNavigation.TipoDashboard != null 
                    && r.IdCategoriaNavigation.TipoDashboard.ToLower() == tipoDashboard.ToLower());
            }

            var today = DateTime.Today;
            var sevenDaysAgo = today.AddDays(-7);

            var stats = await query
                .GroupBy(r => 1)
                .Select(g => new ReporteStatsDto
                {
                    Total = g.Count(),
                    EnProceso = g.Count(r => r.IdEstadoNavigation.Nombre == "En Proceso"),
                    Resueltos = g.Count(r => r.IdEstadoNavigation.Nombre == "Resuelto"),
                    Retrasados = g.Count(r => r.IdEstadoNavigation.Nombre == "Recibido" && r.FechaReporte < sevenDaysAgo),
                    Pendientes = g.Count(r => r.IdEstadoNavigation.Nombre == "Recibido"),
                    Hoy = g.Count(r => r.FechaReporte.Date == today)
                })
                .FirstOrDefaultAsync();

            stats ??= new ReporteStatsDto();

            return Ok(ApiResponse<ReporteStatsDto>.Ok(stats, "Estadísticas obtenidas correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas");
            return StatusCode(500, ApiResponse<ReporteStatsDto>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get monthly report trend (last 12 months)
    /// </summary>
    [HttpGet("tendencia")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReporteChartDataDto>>>> GetTendencia(
        [FromQuery] int? usuarioId = null,
        [FromQuery] string? tipoDashboard = null)
    {
        try
        {
            var twelveMonthsAgo = DateTime.Now.AddMonths(-12);

            var query = _context.Reportes
                .Include(r => r.IdCategoriaNavigation)
                .Where(r => !r.Eliminado && r.FechaReporte >= twelveMonthsAgo);

            if (usuarioId.HasValue)
            {
                query = query.Where(r => r.IdReportante == usuarioId.Value);
            }

            if (!string.IsNullOrWhiteSpace(tipoDashboard))
            {
                query = query.Where(r => r.IdCategoriaNavigation.TipoDashboard != null 
                    && r.IdCategoriaNavigation.TipoDashboard.ToLower() == tipoDashboard.ToLower());
            }

            var data = await query
                .GroupBy(r => new { r.FechaReporte.Year, r.FechaReporte.Month })
                .Select(g => new ReporteChartDataDto
                {
                    Mes = g.Key.Month,
                    Anio = g.Key.Year,
                    Cantidad = g.Count()
                })
                .OrderBy(d => d.Anio)
                .ThenBy(d => d.Mes)
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<ReporteChartDataDto>>.Ok(data, "Tendencia obtenida correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tendencia");
            return StatusCode(500, ApiResponse<IEnumerable<ReporteChartDataDto>>.Fail("Error interno del servidor"));
        }
    }

    /// <summary>
    /// Get report distribution by type
    /// </summary>
    [HttpGet("distribucion-tipos")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReporteTipoDistributionDto>>>> GetDistribucionTipos(
        [FromQuery] int? usuarioId = null,
        [FromQuery] string? tipoDashboard = null)
    {
        try
        {
            var query = _context.Reportes
                .Include(r => r.IdCategoriaNavigation)
                .Where(r => !r.Eliminado);

            if (usuarioId.HasValue)
            {
                query = query.Where(r => r.IdReportante == usuarioId.Value);
            }

            if (!string.IsNullOrWhiteSpace(tipoDashboard))
            {
                query = query.Where(r => r.IdCategoriaNavigation.TipoDashboard != null 
                    && r.IdCategoriaNavigation.TipoDashboard.ToLower() == tipoDashboard.ToLower());
            }

            var data = await query
                .GroupBy(r => r.IdCategoriaNavigation.Nombre)
                .Select(g => new ReporteTipoDistributionDto
                {
                    Tipo = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(d => d.Cantidad)
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<ReporteTipoDistributionDto>>.Ok(data, "Distribución obtenida correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener distribución");
            return StatusCode(500, ApiResponse<IEnumerable<ReporteTipoDistributionDto>>.Fail("Error interno del servidor"));
        }
    }
}

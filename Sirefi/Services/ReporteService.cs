using Microsoft.EntityFrameworkCore;
using Sirefi.Data;
using Sirefi.DTOs;
using Sirefi.Models;

namespace Sirefi.Services;

public class ReporteService : IReporteService
{
    private readonly FormsDbContext _context;

    public ReporteService(FormsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Reporte>> GetAllReportes(int? userId = null, string? role = null)
    {
        var query = _context.Reportes
            .Include(r => r.IdCategoriaNavigation)
            .Include(r => r.IdEstadoNavigation)
            .Include(r => r.IdPrioridadNavigation)
            .Include(r => r.IdReportanteNavigation)
            .Include(r => r.IdAsignadoANavigation)
            .Include(r => r.IdEdificioNavigation)
            .Include(r => r.IdSalonNavigation)
            .Where(r => !r.Eliminado);

        // Filter by user role
        if (role == Constants.Roles.Reportante && userId.HasValue)
        {
            query = query.Where(r => r.IdReportante == userId.Value);
        }

        return await query.OrderByDescending(r => r.FechaReporte).ToListAsync();
    }

    public async Task<IEnumerable<Reporte>> GetReportesByCategory(string tipoDashboard)
    {
        return await _context.Reportes
            .Include(r => r.IdCategoriaNavigation)
            .Include(r => r.IdEstadoNavigation)
            .Include(r => r.IdPrioridadNavigation)
            .Include(r => r.IdReportanteNavigation)
            .Include(r => r.IdAsignadoANavigation)
            .Include(r => r.IdEdificioNavigation)
            .Include(r => r.IdSalonNavigation)
            .Where(r => !r.Eliminado && r.IdCategoriaNavigation.TipoDashboard == tipoDashboard)
            .OrderByDescending(r => r.FechaReporte)
            .ToListAsync();
    }

    public async Task<Reporte?> GetReporteById(int id)
    {
        return await _context.Reportes
            .Include(r => r.IdCategoriaNavigation)
            .Include(r => r.IdEstadoNavigation)
            .Include(r => r.IdPrioridadNavigation)
            .Include(r => r.IdReportanteNavigation)
            .Include(r => r.IdAsignadoANavigation)
            .Include(r => r.IdEdificioNavigation)
            .Include(r => r.IdSalonNavigation)
            .Include(r => r.Archivos)
            .Include(r => r.Comentarios)
            .FirstOrDefaultAsync(r => r.Id == id && !r.Eliminado);
    }

    public async Task<Reporte?> GetReporteByFolio(string folio)
    {
        return await _context.Reportes
            .Include(r => r.IdCategoriaNavigation)
            .Include(r => r.IdEstadoNavigation)
            .Include(r => r.IdPrioridadNavigation)
            .Include(r => r.IdReportanteNavigation)
            .Include(r => r.IdAsignadoANavigation)
            .Include(r => r.IdEdificioNavigation)
            .Include(r => r.IdSalonNavigation)
            .FirstOrDefaultAsync(r => r.Folio == folio && !r.Eliminado);
    }

    public async Task<Reporte> CreateReporte(CreateReporteDto dto, int userId)
    {
        // Get initial state (Recibido)
        var estadoInicial = await _context.Estados
            .FirstOrDefaultAsync(e => e.Nombre == Constants.Estados.Recibido);

        if (estadoInicial == null)
        {
            throw new Exception("Estado inicial 'Recibido' no encontrado");
        }

        // Generate unique folio
        var folio = await GenerateUniqueFolio();

        var reporte = new Reporte
        {
            Folio = folio,
            IdEdificio = dto.IdEdificio,
            IdSalon = dto.IdSalon,
            UbicacionAdicional = dto.UbicacionAdicional,
            IdCategoria = dto.IdCategoria,
            Subcategoria = dto.Subcategoria,
            Titulo = dto.Titulo,
            Descripcion = dto.Descripcion,
            IdPrioridad = dto.IdPrioridad,
            IdEstado = estadoInicial.Id,
            IdReportante = userId,
            FechaReporte = DateTime.Now,
            Eliminado = false
        };

        _context.Reportes.Add(reporte);
        await _context.SaveChangesAsync();

        return reporte;
    }

    public async Task<Reporte> UpdateReporte(int id, UpdateReporteDto dto, int userId)
    {
        var reporte = await GetReporteById(id);
        if (reporte == null)
        {
            throw new Exception("Reporte no encontrado");
        }

        reporte.IdEdificio = dto.IdEdificio;
        reporte.IdSalon = dto.IdSalon;
        reporte.UbicacionAdicional = dto.UbicacionAdicional;
        reporte.IdCategoria = dto.IdCategoria;
        reporte.Subcategoria = dto.Subcategoria;
        reporte.Titulo = dto.Titulo;
        reporte.Descripcion = dto.Descripcion;
        reporte.IdPrioridad = dto.IdPrioridad;
        reporte.IdEstado = dto.IdEstado;
        reporte.IdAsignadoA = dto.IdAsignadoA;
        reporte.FechaActualizacion = DateTime.Now;
        reporte.ActualizadoPor = userId;

        await _context.SaveChangesAsync();

        return reporte;
    }

    public async Task<bool> DeleteReporte(int id)
    {
        var reporte = await GetReporteById(id);
        if (reporte == null)
        {
            return false;
        }

        reporte.Eliminado = true;
        reporte.FechaEliminacion = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Dictionary<string, object>> GetDashboardStats(string? tipoDashboard = null)
    {
        var query = _context.Reportes
            .Include(r => r.IdCategoriaNavigation)
            .Include(r => r.IdEstadoNavigation)
            .Where(r => !r.Eliminado);

        if (!string.IsNullOrEmpty(tipoDashboard))
        {
            query = query.Where(r => r.IdCategoriaNavigation.TipoDashboard == tipoDashboard);
        }

        var reportes = await query.ToListAsync();

        var stats = new Dictionary<string, object>
        {
            ["total"] = reportes.Count,
            ["recibidos"] = reportes.Count(r => r.IdEstadoNavigation.Nombre == Constants.Estados.Recibido),
            ["en_proceso"] = reportes.Count(r => r.IdEstadoNavigation.Nombre == Constants.Estados.EnProceso),
            ["solucionados"] = reportes.Count(r => r.IdEstadoNavigation.Nombre == Constants.Estados.Solucionado),
            ["cancelados"] = reportes.Count(r => r.IdEstadoNavigation.Nombre == Constants.Estados.Cancelado),
            ["por_categoria"] = reportes
                .GroupBy(r => r.IdCategoriaNavigation.Nombre)
                .Select(g => new { categoria = g.Key, total = g.Count() })
                .ToList()
        };

        return stats;
    }

    private async Task<string> GenerateUniqueFolio()
    {
        var prefix = "REP";
        var date = DateTime.Now.ToString("yyyyMMdd");
        var lastFolio = await _context.Reportes
            .Where(r => r.Folio.StartsWith(prefix + date))
            .OrderByDescending(r => r.Folio)
            .Select(r => r.Folio)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (!string.IsNullOrEmpty(lastFolio) && lastFolio.Length >= 4)
        {
            var lastSequence = lastFolio.Substring(lastFolio.Length - 4);
            if (int.TryParse(lastSequence, out int num))
            {
                sequence = num + 1;
            }
        }

        return $"{prefix}{date}{sequence:D4}";
    }
}

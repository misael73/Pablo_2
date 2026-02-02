using Sirefi.DTOs;
using Sirefi.Models;

namespace Sirefi.Services;

public interface IReporteService
{
    Task<IEnumerable<Reporte>> GetAllReportes(int? userId = null, string? role = null);
    Task<IEnumerable<Reporte>> GetReportesByCategory(string tipoDashboard);
    Task<Reporte?> GetReporteById(int id);
    Task<Reporte?> GetReporteByFolio(string folio);
    Task<Reporte> CreateReporte(CreateReporteDto dto, int userId);
    Task<Reporte> UpdateReporte(int id, UpdateReporteDto dto, int userId);
    Task<bool> DeleteReporte(int id);
    Task<Dictionary<string, object>> GetDashboardStats(string? tipoDashboard = null);
}

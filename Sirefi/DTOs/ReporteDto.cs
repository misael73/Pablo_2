namespace Sirefi.DTOs;

public class ReporteDto
{
    public int Id { get; set; }
    public string Folio { get; set; } = null!;
    public int? IdEdificio { get; set; }
    public string? EdificioNombre { get; set; }
    public int? IdSalon { get; set; }
    public string? SalonNombre { get; set; }
    public string? UbicacionAdicional { get; set; }
    public int IdCategoria { get; set; }
    public string? CategoriaNombre { get; set; }
    public string? Subcategoria { get; set; }
    public string? Titulo { get; set; }
    public string Descripcion { get; set; } = null!;
    public int IdPrioridad { get; set; }
    public string? PrioridadNombre { get; set; }
    public string? PrioridadColor { get; set; }
    public int IdEstado { get; set; }
    public string? EstadoNombre { get; set; }
    public string? EstadoColor { get; set; }
    public int IdReportante { get; set; }
    public string? ReportanteNombre { get; set; }
    public string? ReportanteCorreo { get; set; }
    public string? ReportanteDepartamento { get; set; }
    public int? IdAsignadoA { get; set; }
    public string? TecnicoNombre { get; set; }
    public DateTime FechaReporte { get; set; }
    public DateTime? FechaAsignacion { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
    public string? UltimaAccion { get; set; }
}

public class CreateReporteDto
{
    public int IdEdificio { get; set; }
    public int? IdSalon { get; set; }
    public string? UbicacionAdicional { get; set; }
    public int IdCategoria { get; set; }
    public string? Subcategoria { get; set; }
    public string? Titulo { get; set; }
    public string Descripcion { get; set; } = null!;
    public int IdReportante { get; set; }
}

public class UpdateReporteDto
{
    public int IdEstado { get; set; }
    public int IdPrioridad { get; set; }
    public int? IdAsignadoA { get; set; }
    public string? Comentario { get; set; }
    public int ActualizadoPor { get; set; }
}

public class ReporteStatsDto
{
    public int Total { get; set; }
    public int EnProceso { get; set; }
    public int Resueltos { get; set; }
    public int Retrasados { get; set; }
    public int Pendientes { get; set; }
    public int Hoy { get; set; }
}

public class ReporteChartDataDto
{
    public int Mes { get; set; }
    public int Anio { get; set; }
    public int Cantidad { get; set; }
}

public class ReporteTipoDistributionDto
{
    public string Tipo { get; set; } = null!;
    public int Cantidad { get; set; }
}

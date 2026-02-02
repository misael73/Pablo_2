namespace BlazorApp1.Models;

public class ReporteModel
{
    public int Id { get; set; }
    public string Folio { get; set; } = string.Empty;
    public string? Titulo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? EstadoColor { get; set; }
    public string Prioridad { get; set; } = string.Empty;
    public string? PrioridadColor { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public DateTime FechaReporte { get; set; }
    public string? Edificio { get; set; }
    public string? Salon { get; set; }
    public string ReportanteNombre { get; set; } = string.Empty;
    public string? TecnicoNombre { get; set; }
}

public class CreateReporteModel
{
    public int? IdEdificio { get; set; }
    public int? IdSalon { get; set; }
    public string? UbicacionAdicional { get; set; }
    public int IdCategoria { get; set; }
    public string? Subcategoria { get; set; }
    public string? Titulo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int IdPrioridad { get; set; }
}

public class DashboardStats
{
    public int Total { get; set; }
    public int Recibidos { get; set; }
    public int EnProceso { get; set; }
    public int Solucionados { get; set; }
    public int Cancelados { get; set; }
}

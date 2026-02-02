namespace Sirefi.DTOs;

public class CreateReporteDto
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

public class UpdateReporteDto
{
    public int? IdEdificio { get; set; }
    public int? IdSalon { get; set; }
    public string? UbicacionAdicional { get; set; }
    public int IdCategoria { get; set; }
    public string? Subcategoria { get; set; }
    public string? Titulo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int IdPrioridad { get; set; }
    public int IdEstado { get; set; }
    public int? IdAsignadoA { get; set; }
}

public class ReporteResponseDto
{
    public int Id { get; set; }
    public string Folio { get; set; } = string.Empty;
    public string? Titulo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Prioridad { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public DateTime FechaReporte { get; set; }
    public string? Edificio { get; set; }
    public string? Salon { get; set; }
    public string ReportanteNombre { get; set; } = string.Empty;
    public string? TecnicoNombre { get; set; }
}

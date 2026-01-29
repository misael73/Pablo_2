using System;
using System.Collections.Generic;

namespace Sirefi.Models;

public partial class VwReportesCompleto
{
    public int Id { get; set; }

    public string Folio { get; set; } = null!;

    public string? Titulo { get; set; }

    public string Descripcion { get; set; } = null!;

    public string Edificio { get; set; } = null!;

    public string Salon { get; set; } = null!;

    public string? UbicacionAdicional { get; set; }

    public string Categoria { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public string? EstadoColor { get; set; }

    public string Prioridad { get; set; } = null!;

    public string? PrioridadColor { get; set; }

    public string ReportanteNombre { get; set; } = null!;

    public string? TecnicoNombre { get; set; }

    public DateTime FechaReporte { get; set; }

    public DateTime? FechaAsignacion { get; set; }

    public bool Eliminado { get; set; }
}

using System;
using System.Collections.Generic;

namespace Sirefi.Models;

public partial class Estado
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int Orden { get; set; }

    public bool EsFinal { get; set; }

    public string? Color { get; set; }

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<HistorialEstado> HistorialEstadoIdEstadoAnteriorNavigations { get; set; } = new List<HistorialEstado>();

    public virtual ICollection<HistorialEstado> HistorialEstadoIdEstadoNuevoNavigations { get; set; } = new List<HistorialEstado>();

    public virtual ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();
}

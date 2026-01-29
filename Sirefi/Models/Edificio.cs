using System;
using System.Collections.Generic;

namespace Sirefi.Models;

public partial class Edificio
{
    public int Id { get; set; }

    public string? Codigo { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? Ubicacion { get; set; }

    public int? Pisos { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public int? CreadoPor { get; set; }

    public int? ActualizadoPor { get; set; }

    public virtual ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();

    public virtual ICollection<Salone> Salones { get; set; } = new List<Salone>();
}

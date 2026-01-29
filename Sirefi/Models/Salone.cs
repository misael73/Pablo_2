using System;
using System.Collections.Generic;

namespace Sirefi.Models;

public partial class Salone
{
    public int Id { get; set; }

    public int IdEdificio { get; set; }

    public string? Codigo { get; set; }

    public string Nombre { get; set; } = null!;

    public string? TipoEspacio { get; set; }

    public int? Capacidad { get; set; }

    public int? Piso { get; set; }

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public int? CreadoPor { get; set; }

    public int? ActualizadoPor { get; set; }

    public virtual Edificio IdEdificioNavigation { get; set; } = null!;

    public virtual ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();
}

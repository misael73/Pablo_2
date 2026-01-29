using System;
using System.Collections.Generic;

namespace Sirefi.Models;

public partial class Categoria
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? TipoDashboard { get; set; }

    public string? Descripcion { get; set; }

    public string? Icono { get; set; }

    public string? Color { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();
}

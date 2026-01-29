using System;
using System.Collections.Generic;

namespace Sirefi.Models;

public partial class Prioridade
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int Nivel { get; set; }

    public string? Color { get; set; }

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();
}

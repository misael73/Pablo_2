using System;
using System.Collections.Generic;

namespace Sirefi.Models;

public partial class Notificacione
{
    public int Id { get; set; }

    public int IdUsuario { get; set; }

    public int? IdReporte { get; set; }

    public string Tipo { get; set; } = null!;

    public string Titulo { get; set; } = null!;

    public string? Mensaje { get; set; }

    public bool Leido { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaLeido { get; set; }

    public virtual Reporte? IdReporteNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}

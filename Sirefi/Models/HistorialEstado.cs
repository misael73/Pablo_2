using System;
using System.Collections.Generic;

namespace Sirefi.Models;

public partial class HistorialEstado
{
    public int Id { get; set; }

    public int IdReporte { get; set; }

    public int? IdEstadoAnterior { get; set; }

    public int IdEstadoNuevo { get; set; }

    public int IdUsuario { get; set; }

    public string? Comentario { get; set; }

    public DateTime FechaCambio { get; set; }

    public virtual Estado? IdEstadoAnteriorNavigation { get; set; }

    public virtual Estado IdEstadoNuevoNavigation { get; set; } = null!;

    public virtual Reporte IdReporteNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}

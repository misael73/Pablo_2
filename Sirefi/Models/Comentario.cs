using System;
using System.Collections.Generic;

namespace Sirefi.Models;

public partial class Comentario
{
    public int Id { get; set; }

    public int IdReporte { get; set; }

    public int IdUsuario { get; set; }

    public string Comentario1 { get; set; } = null!;

    public string? Tipo { get; set; }

    public bool Publico { get; set; }

    public DateTime FechaComentario { get; set; }

    public bool Editado { get; set; }

    public DateTime? FechaEdicion { get; set; }

    public bool Eliminado { get; set; }

    public virtual ICollection<Archivo> Archivos { get; set; } = new List<Archivo>();

    public virtual Reporte IdReporteNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace Sirefi.Models;

public partial class Archivo
{
    public int Id { get; set; }

    public int? IdReporte { get; set; }

    public int? IdComentario { get; set; }

    public string NombreOriginal { get; set; } = null!;

    public string NombreArchivo { get; set; } = null!;

    public string Ruta { get; set; } = null!;

    public string? TipoMime { get; set; }

    public long? TamanoBytes { get; set; }

    public int IdUsuario { get; set; }

    public DateTime FechaSubida { get; set; }

    public bool Eliminado { get; set; }

    public virtual Comentario? IdComentarioNavigation { get; set; }

    public virtual Reporte? IdReporteNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}

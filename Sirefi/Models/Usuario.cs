using System;
using System.Collections.Generic;

namespace Sirefi.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string? Contrasena { get; set; }

    public string Rol { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Foto { get; set; }

    public string? Departamento { get; set; }

    public bool Activo { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual ICollection<Archivo> Archivos { get; set; } = new List<Archivo>();

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual ICollection<HistorialEstado> HistorialEstados { get; set; } = new List<HistorialEstado>();

    public virtual ICollection<Notificacione> Notificaciones { get; set; } = new List<Notificacione>();

    public virtual ICollection<Reporte> ReporteActualizadoPorNavigations { get; set; } = new List<Reporte>();

    public virtual ICollection<Reporte> ReporteIdAsignadoANavigations { get; set; } = new List<Reporte>();

    public virtual ICollection<Reporte> ReporteIdReportanteNavigations { get; set; } = new List<Reporte>();
}

using System;
using System.Collections.Generic;

namespace Sirefi.Models;

public partial class Reporte
{
    public int Id { get; set; }

    public string Folio { get; set; } = null!;

    public int? IdEdificio { get; set; }

    public int? IdSalon { get; set; }

    public string? UbicacionAdicional { get; set; }

    public int IdCategoria { get; set; }

    public string? Subcategoria { get; set; }

    public string? Titulo { get; set; }

    public string Descripcion { get; set; } = null!;

    public int IdPrioridad { get; set; }

    public int IdEstado { get; set; }

    public int IdReportante { get; set; }

    public int? IdAsignadoA { get; set; }

    public DateTime FechaReporte { get; set; }

    public DateTime? FechaAsignacion { get; set; }

    public DateTime? FechaInicioAtencion { get; set; }

    public DateTime? FechaFinalizacion { get; set; }

    public int? TiempoRespuestaMinutos { get; set; }

    public int? TiempoResolucionMinutos { get; set; }

    public int? Calificacion { get; set; }

    public string? ComentarioCalificacion { get; set; }

    public decimal? CostoEstimado { get; set; }

    public decimal? CostoReal { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public int? ActualizadoPor { get; set; }

    public bool Eliminado { get; set; }

    public DateTime? FechaEliminacion { get; set; }

    public virtual Usuario? ActualizadoPorNavigation { get; set; }

    public virtual ICollection<Archivo> Archivos { get; set; } = new List<Archivo>();

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual ICollection<HistorialEstado> HistorialEstados { get; set; } = new List<HistorialEstado>();

    public virtual Usuario? IdAsignadoANavigation { get; set; }

    public virtual Categoria IdCategoriaNavigation { get; set; } = null!;

    public virtual Edificio? IdEdificioNavigation { get; set; }

    public virtual Estado IdEstadoNavigation { get; set; } = null!;

    public virtual Prioridade IdPrioridadNavigation { get; set; } = null!;

    public virtual Usuario IdReportanteNavigation { get; set; } = null!;

    public virtual Salone? IdSalonNavigation { get; set; }

    public virtual ICollection<Notificacione> Notificaciones { get; set; } = new List<Notificacione>();
}

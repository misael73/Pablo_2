namespace BlazorApp1.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Rol { get; set; } = "";
    public string? Telefono { get; set; }
    public string? Foto { get; set; }
    public string? Departamento { get; set; }
    public bool Activo { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class ReporteDto
{
    public int Id { get; set; }
    public string Folio { get; set; } = "";
    public int? IdEdificio { get; set; }
    public string? EdificioNombre { get; set; }
    public int? IdSalon { get; set; }
    public string? SalonNombre { get; set; }
    public string? UbicacionAdicional { get; set; }
    public int IdCategoria { get; set; }
    public string? CategoriaNombre { get; set; }
    public string? Subcategoria { get; set; }
    public string? Titulo { get; set; }
    public string Descripcion { get; set; } = "";
    public int IdPrioridad { get; set; }
    public string? PrioridadNombre { get; set; }
    public string? PrioridadColor { get; set; }
    public int IdEstado { get; set; }
    public string? EstadoNombre { get; set; }
    public string? EstadoColor { get; set; }
    public int IdReportante { get; set; }
    public string? ReportanteNombre { get; set; }
    public string? ReportanteCorreo { get; set; }
    public string? ReportanteDepartamento { get; set; }
    public int? IdAsignadoA { get; set; }
    public string? TecnicoNombre { get; set; }
    public DateTime FechaReporte { get; set; }
    public DateTime? FechaAsignacion { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
    public string? UltimaAccion { get; set; }
}

public class ReporteStatsDto
{
    public int Total { get; set; }
    public int EnProceso { get; set; }
    public int Resueltos { get; set; }
    public int Retrasados { get; set; }
    public int Pendientes { get; set; }
    public int Hoy { get; set; }
}

public class CreateReporteDto
{
    public int IdEdificio { get; set; }
    public int? IdSalon { get; set; }
    public string? UbicacionAdicional { get; set; }
    public int IdCategoria { get; set; }
    public string? Subcategoria { get; set; }
    public string? Titulo { get; set; }
    public string Descripcion { get; set; } = "";
    public int IdReportante { get; set; }
}

public class UpdateReporteDto
{
    public int IdEstado { get; set; }
    public int IdPrioridad { get; set; }
    public int? IdAsignadoA { get; set; }
    public string? Comentario { get; set; }
    public int ActualizadoPor { get; set; }
}

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? TipoDashboard { get; set; }
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public string? Color { get; set; }
    public bool Activo { get; set; }
}

public class EdificioDto
{
    public int Id { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public string? Ubicacion { get; set; }
    public int? Pisos { get; set; }
    public bool Activo { get; set; }
    public int SalonesCount { get; set; }
}

public class SalonDto
{
    public int Id { get; set; }
    public int IdEdificio { get; set; }
    public string? EdificioNombre { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = "";
    public string? TipoEspacio { get; set; }
    public int? Capacidad { get; set; }
    public int? Piso { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}

public class ComentarioDto
{
    public int Id { get; set; }
    public int IdReporte { get; set; }
    public int IdUsuario { get; set; }
    public string? UsuarioNombre { get; set; }
    public string? UsuarioRol { get; set; }
    public string Comentario { get; set; } = "";
    public string? Tipo { get; set; }
    public bool Publico { get; set; }
    public DateTime FechaComentario { get; set; }
    public bool Editado { get; set; }
}

public class CreateComentarioDto
{
    public int IdReporte { get; set; }
    public int IdUsuario { get; set; }
    public string Comentario { get; set; } = "";
    public string? Tipo { get; set; }
    public bool Publico { get; set; } = true;
}

public class GoogleLoginDto
{
    public string IdToken { get; set; } = "";
}

public class EstadoPrioridadDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public string? Color { get; set; }
    public bool Activo { get; set; }
}

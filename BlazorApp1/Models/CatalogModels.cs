namespace BlazorApp1.Models;

public class EdificioModel
{
    public int Id { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}

public class SalonModel
{
    public int Id { get; set; }
    public int IdEdificio { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}

public class CategoriaModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? TipoDashboard { get; set; }
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public string? Color { get; set; }
    public bool Activo { get; set; }
}

public class PrioridadModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Color { get; set; }
}

public class EstadoModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Color { get; set; }
}

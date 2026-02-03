namespace Sirefi.DTOs;

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? TipoDashboard { get; set; }
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public string? Color { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class CreateCategoriaDto
{
    public string Nombre { get; set; } = null!;
    public string TipoDashboard { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public string? Color { get; set; }
    public bool Activo { get; set; } = true;
}

public class UpdateCategoriaDto
{
    public string Nombre { get; set; } = null!;
    public string TipoDashboard { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public string? Color { get; set; }
    public bool Activo { get; set; }
}

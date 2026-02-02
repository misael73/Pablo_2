namespace Sirefi.DTOs;

public class CreateCategoriaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? TipoDashboard { get; set; }
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public string? Color { get; set; }
    public bool Activo { get; set; } = true;
}

public class UpdateCategoriaDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? TipoDashboard { get; set; }
    public string? Descripcion { get; set; }
    public string? Icono { get; set; }
    public string? Color { get; set; }
    public bool Activo { get; set; }
}

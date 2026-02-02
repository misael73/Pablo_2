namespace Sirefi.DTOs;

public class CreateEdificioDto
{
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Ubicacion { get; set; }
    public int? Pisos { get; set; }
    public bool Activo { get; set; } = true;
}

public class UpdateEdificioDto
{
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Ubicacion { get; set; }
    public int? Pisos { get; set; }
    public bool Activo { get; set; }
}

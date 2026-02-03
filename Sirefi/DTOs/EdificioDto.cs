namespace Sirefi.DTOs;

public class EdificioDto
{
    public int Id { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string? Ubicacion { get; set; }
    public int? Pisos { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int SalonesCount { get; set; }
}

public class CreateEdificioDto
{
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string? Ubicacion { get; set; }
    public int? Pisos { get; set; }
    public bool Activo { get; set; } = true;
}

public class UpdateEdificioDto
{
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string? Ubicacion { get; set; }
    public int? Pisos { get; set; }
    public bool Activo { get; set; }
}

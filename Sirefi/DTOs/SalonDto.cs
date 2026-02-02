namespace Sirefi.DTOs;

public class CreateSalonDto
{
    public int IdEdificio { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? Piso { get; set; }
    public string? TipoEspacio { get; set; }
    public int? Capacidad { get; set; }
    public bool Activo { get; set; } = true;
}

public class UpdateSalonDto
{
    public int IdEdificio { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? Piso { get; set; }
    public string? TipoEspacio { get; set; }
    public int? Capacidad { get; set; }
    public bool Activo { get; set; }
}

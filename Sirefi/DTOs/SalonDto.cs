namespace Sirefi.DTOs;

public class SalonDto
{
    public int Id { get; set; }
    public int IdEdificio { get; set; }
    public string? EdificioNombre { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = null!;
    public string? TipoEspacio { get; set; }
    public int? Capacidad { get; set; }
    public int? Piso { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class CreateSalonDto
{
    public int IdEdificio { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = null!;
    public string? TipoEspacio { get; set; }
    public int? Capacidad { get; set; }
    public int? Piso { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
}

public class UpdateSalonDto
{
    public int IdEdificio { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = null!;
    public string? TipoEspacio { get; set; }
    public int? Capacidad { get; set; }
    public int? Piso { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}

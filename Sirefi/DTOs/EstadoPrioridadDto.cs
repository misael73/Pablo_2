namespace Sirefi.DTOs;

public class EstadoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public int Orden { get; set; }
    public bool EsFinal { get; set; }
    public string? Color { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}

public class PrioridadDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public int Nivel { get; set; }
    public string? Color { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}

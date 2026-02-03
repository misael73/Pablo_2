namespace Sirefi.DTOs;

public class ComentarioDto
{
    public int Id { get; set; }
    public int IdReporte { get; set; }
    public int IdUsuario { get; set; }
    public string? UsuarioNombre { get; set; }
    public string? UsuarioRol { get; set; }
    public string Comentario { get; set; } = null!;
    public string? Tipo { get; set; }
    public bool Publico { get; set; }
    public DateTime FechaComentario { get; set; }
    public bool Editado { get; set; }
}

public class CreateComentarioDto
{
    public int IdReporte { get; set; }
    public int IdUsuario { get; set; }
    public string Comentario { get; set; } = null!;
    public string? Tipo { get; set; }
    public bool Publico { get; set; } = true;
}

public class UpdateComentarioDto
{
    public string Comentario { get; set; } = null!;
}

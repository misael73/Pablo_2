namespace Sirefi.DTOs;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string Rol { get; set; } = null!;
    public string? Telefono { get; set; }
    public string? Foto { get; set; }
    public string? Departamento { get; set; }
    public bool Activo { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class CreateUsuarioDto
{
    public string Nombre { get; set; } = null!;
    public string Correo { get; set; } = null!;
    public string Rol { get; set; } = "reportante";
    public string? Telefono { get; set; }
    public string? Foto { get; set; }
    public string? Departamento { get; set; }
    public bool Activo { get; set; } = true;
}

public class UpdateUsuarioDto
{
    public string Nombre { get; set; } = null!;
    public string Rol { get; set; } = null!;
    public string? Telefono { get; set; }
    public string? Foto { get; set; }
    public string? Departamento { get; set; }
    public bool Activo { get; set; }
}

public class GoogleLoginDto
{
    public string IdToken { get; set; } = null!;
}

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public UsuarioDto? Usuario { get; set; }
}

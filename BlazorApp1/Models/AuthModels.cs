namespace BlazorApp1.Models;

public class UserModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Foto { get; set; }
    public string Rol { get; set; } = string.Empty;
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public UserModel? Usuario { get; set; }
}

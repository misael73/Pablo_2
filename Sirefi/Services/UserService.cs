using Microsoft.EntityFrameworkCore;
using Sirefi.Data;
using Sirefi.Models;

namespace Sirefi.Services;

public class UserService : IUserService
{
    private readonly FormsDbContext _context;

    public UserService(FormsDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetUserById(int id)
    {
        return await _context.Usuarios.FindAsync(id);
    }

    public async Task<Usuario?> GetUserByEmail(string email)
    {
        return await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == email);
    }

    public async Task<IEnumerable<Usuario>> GetAllUsers()
    {
        return await _context.Usuarios
            .Where(u => u.Activo)
            .OrderBy(u => u.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Usuario>> GetUsersByRole(string role)
    {
        return await _context.Usuarios
            .Where(u => u.Activo && u.Rol == role)
            .OrderBy(u => u.Nombre)
            .ToListAsync();
    }

    public async Task<Usuario> CreateUser(Usuario user)
    {
        user.FechaCreacion = DateTime.Now;
        user.Activo = true;

        _context.Usuarios.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<Usuario> UpdateUser(Usuario user)
    {
        user.FechaActualizacion = DateTime.Now;

        _context.Usuarios.Update(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<bool> DeleteUser(int id)
    {
        var user = await GetUserById(id);
        if (user == null)
        {
            return false;
        }

        user.Activo = false;
        user.FechaActualizacion = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }
}

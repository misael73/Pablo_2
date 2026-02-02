using Sirefi.Models;

namespace Sirefi.Services;

public interface IUserService
{
    Task<Usuario?> GetUserById(int id);
    Task<Usuario?> GetUserByEmail(string email);
    Task<IEnumerable<Usuario>> GetAllUsers();
    Task<IEnumerable<Usuario>> GetUsersByRole(string role);
    Task<Usuario> CreateUser(Usuario user);
    Task<Usuario> UpdateUser(Usuario user);
    Task<bool> DeleteUser(int id);
}

using Sirefi.DTOs;
using Sirefi.Models;

namespace Sirefi.Services;

public interface IAuthService
{
    Task<LoginResponse> ValidateGoogleToken(string idToken);
    Task<Usuario?> GetUserByEmail(string email);
    Task<Usuario> CreateOrUpdateUser(GoogleUserInfo userInfo);
}

using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorApp1.Models;
using BlazorApp1.Services;

namespace BlazorApp1.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ApiService _apiService;
    private UserModel? _currentUser;

    public CustomAuthStateProvider(ApiService apiService)
    {
        _apiService = apiService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();

        try
        {
            var user = await _apiService.GetCurrentUser();
            if (user != null)
            {
                _currentUser = user;
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Nombre),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Rol)
                };
                identity = new ClaimsIdentity(claims, "apiauth");
            }
        }
        catch
        {
            // User is not authenticated
        }

        var claimsPrincipal = new ClaimsPrincipal(identity);
        return new AuthenticationState(claimsPrincipal);
    }

    public async Task<LoginResponse> Login(string idToken)
    {
        var loginResponse = await _apiService.ValidateGoogleToken(idToken);
        
        if (loginResponse.Success && loginResponse.Usuario != null)
        {
            _currentUser = loginResponse.Usuario;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        return loginResponse;
    }

    public async Task Logout()
    {
        await _apiService.Logout();
        _currentUser = null;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }

    public UserModel? CurrentUser => _currentUser;
}

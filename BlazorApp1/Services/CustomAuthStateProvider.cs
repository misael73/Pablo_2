using Blazored.LocalStorage;
using BlazorApp1.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace BlazorApp1.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly ApiService _apiService;

    public CustomAuthStateProvider(ILocalStorageService localStorage, ApiService apiService)
    {
        _localStorage = localStorage;
        _apiService = apiService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var userJson = await _localStorage.GetItemAsStringAsync("user");
        
        if (string.IsNullOrEmpty(userJson))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var user = JsonSerializer.Deserialize<UsuarioDto>(userJson);
        
        if (user == null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Nombre),
            new Claim(ClaimTypes.Email, user.Correo),
            new Claim(ClaimTypes.Role, user.Rol),
            new Claim("foto", user.Foto ?? "")
        };

        var identity = new ClaimsIdentity(claims, "google");
        var principal = new ClaimsPrincipal(identity);

        return new AuthenticationState(principal);
    }

    public async Task<bool> LoginWithGoogleAsync(string idToken)
    {
        var response = await _apiService.GoogleLoginAsync(idToken);
        
        if (response?.Success == true && response.Data != null)
        {
            var userJson = JsonSerializer.Serialize(response.Data);
            await _localStorage.SetItemAsStringAsync("user", userJson);
            
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return true;
        }
        
        return false;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("user");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<UsuarioDto?> GetCurrentUserAsync()
    {
        var userJson = await _localStorage.GetItemAsStringAsync("user");
        
        if (string.IsNullOrEmpty(userJson))
        {
            return null;
        }

        return JsonSerializer.Deserialize<UsuarioDto>(userJson);
    }
}

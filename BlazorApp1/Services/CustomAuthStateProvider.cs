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
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CustomAuthStateProvider(ILocalStorageService localStorage, ApiService apiService)
    {
        _localStorage = localStorage;
        _apiService = apiService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userJson = await _localStorage.GetItemAsStringAsync("user");
            
            if (string.IsNullOrEmpty(userJson))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var user = JsonSerializer.Deserialize<UsuarioDto>(userJson, JsonOptions);
            
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
        catch (JsonException)
        {
            // If deserialization fails, clear the invalid data and return anonymous
            await _localStorage.RemoveItemAsync("user");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async Task<bool> LoginWithGoogleAsync(string idToken)
    {
        var response = await _apiService.GoogleLoginAsync(idToken);
        
        if (response?.Success == true && response.Data != null)
        {
            var userJson = JsonSerializer.Serialize(response.Data, JsonOptions);
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
        try
        {
            var userJson = await _localStorage.GetItemAsStringAsync("user");
            
            if (string.IsNullOrEmpty(userJson))
            {
                return null;
            }

            return JsonSerializer.Deserialize<UsuarioDto>(userJson, JsonOptions);
        }
        catch (JsonException)
        {
            // If deserialization fails, return null
            return null;
        }
    }
}

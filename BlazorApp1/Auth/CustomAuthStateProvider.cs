using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorApp1.Models;
using BlazorApp1.Services;

namespace BlazorApp1.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ApiService _apiService;
    private UserModel? _currentUser;
    private AuthenticationState? _cachedAuthState;
    private bool _isCheckingAuth = false;
    private DateTime _lastAuthCheck = DateTime.MinValue;
    private readonly TimeSpan _authCheckCooldown = TimeSpan.FromSeconds(5);

    public CustomAuthStateProvider(ApiService apiService)
    {
        _apiService = apiService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Return cached state if available and recent
        if (_cachedAuthState != null && (DateTime.UtcNow - _lastAuthCheck) < _authCheckCooldown)
        {
            return _cachedAuthState;
        }

        // Prevent concurrent auth checks
        if (_isCheckingAuth)
        {
            // Wait a bit and return cached or anonymous
            await Task.Delay(100);
            return _cachedAuthState ?? CreateAnonymousState();
        }

        _isCheckingAuth = true;
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
        catch (Exception ex)
        {
            // User is not authenticated or backend is not available
            Console.WriteLine($"Auth check failed (expected if not logged in or backend is down): {ex.Message}");
        }
        finally
        {
            _isCheckingAuth = false;
            _lastAuthCheck = DateTime.UtcNow;
        }

        var claimsPrincipal = new ClaimsPrincipal(identity);
        _cachedAuthState = new AuthenticationState(claimsPrincipal);
        return _cachedAuthState;
    }

    private AuthenticationState CreateAnonymousState()
    {
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public async Task<LoginResponse> Login(string idToken)
    {
        var loginResponse = await _apiService.ValidateGoogleToken(idToken);
        
        if (loginResponse.Success && loginResponse.Usuario != null)
        {
            _currentUser = loginResponse.Usuario;
            _cachedAuthState = null; // Clear cache to force refresh
            _lastAuthCheck = DateTime.MinValue; // Reset cooldown
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        return loginResponse;
    }

    public async Task Logout()
    {
        await _apiService.Logout();
        _currentUser = null;
        _cachedAuthState = null; // Clear cache
        _lastAuthCheck = DateTime.MinValue; // Reset cooldown
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }

    public UserModel? CurrentUser => _currentUser;
}

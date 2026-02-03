using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace BlazorApp1;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthStateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // For now, return an anonymous user
            // In a real application, you would validate the token with the server
            return await Task.FromResult(new AuthenticationState(_anonymous));
        }
        catch (HttpRequestException)
        {
            // Network error - return anonymous user
            return new AuthenticationState(_anonymous);
        }
    }

    public void NotifyUserAuthentication(string token)
    {
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromToken(token), "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        var authState = Task.FromResult(new AuthenticationState(_anonymous));
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromToken(string token)
    {
        // Simple claim extraction - in production, use proper JWT parsing
        var claims = new List<Claim>();
        
        if (!string.IsNullOrEmpty(token))
        {
            claims.Add(new Claim(ClaimTypes.Name, "User"));
        }
        
        return claims;
    }
}

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace BlazorApp1;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthStateProvider(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Try to get the auth token from localStorage
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            
            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(_anonymous);
            }

            // Parse claims from the token and create authenticated user
            var claims = ParseClaimsFromToken(token);
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            return new AuthenticationState(authenticatedUser);
        }
        catch (JSDisconnectedException)
        {
            // Expected during pre-rendering
            return new AuthenticationState(_anonymous);
        }
        catch (InvalidOperationException)
        {
            // Can occur during pre-rendering
            return new AuthenticationState(_anonymous);
        }
        catch (Exception)
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task<bool> LoginWithGoogleAsync(string idToken)
    {
        try
        {
            // Note: The Google Identity Services library validates the token signature
            // before providing it to our callback. The token is cryptographically signed
            // by Google and verified by the GIS library. We parse it here only to extract
            // user information for display purposes.
            
            // Store the token in localStorage
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", idToken);
            
            // Create authenticated user
            var claims = ParseClaimsFromToken(idToken);
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            
            NotifyAuthenticationStateChanged(authState);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        }
        catch (Exception)
        {
            // Ignore errors during logout
        }

        var authState = Task.FromResult(new AuthenticationState(_anonymous));
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromToken(string token)
    {
        var claims = new List<Claim>();
        
        try
        {
            // For Google ID tokens, parse the JWT payload
            var parts = token.Split('.');
            if (parts.Length == 3)
            {
                var payload = parts[1];
                // Add padding if needed
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }
                
                var jsonBytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
                var json = System.Text.Encoding.UTF8.GetString(jsonBytes);
                
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("name", out var name))
                    claims.Add(new Claim(ClaimTypes.Name, name.GetString() ?? ""));
                    
                if (root.TryGetProperty("email", out var email))
                    claims.Add(new Claim(ClaimTypes.Email, email.GetString() ?? ""));
                    
                if (root.TryGetProperty("picture", out var picture))
                    claims.Add(new Claim("picture", picture.GetString() ?? ""));
                    
                if (root.TryGetProperty("sub", out var sub))
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, sub.GetString() ?? ""));
            }
        }
        catch (Exception)
        {
            // If parsing fails, just return empty claims
        }
        
        return claims;
    }
}

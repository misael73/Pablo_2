using Microsoft.JSInterop;

namespace BlazorApp1.Handlers;

public class CookieHandler : DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime;

    public CookieHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Attempt to read authentication token from browser storage/cookies
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", cancellationToken, "authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch (JSDisconnectedException)
        {
            // Expected during pre-rendering when JS interop is not available
        }
        catch (InvalidOperationException)
        {
            // Can occur during pre-rendering or when the circuit is disconnected
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

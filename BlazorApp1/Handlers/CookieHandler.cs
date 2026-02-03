using System.Net.Http;

namespace BlazorApp1.Handlers;

/// <summary>
/// Custom HTTP message handler for Blazor WebAssembly that ensures credentials (cookies) are included in requests
/// </summary>
public class CookieHandler : DelegatingHandler
{
    public CookieHandler()
    {
        InnerHandler = new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Note: In Blazor WebAssembly, the actual fetch is done by the browser
        // We set options that will be translated to fetch options
        
        // This option will cause the browser to include credentials (cookies)
        request.Options.TryAdd("WebAssemblyFetchOptions", new
        {
            credentials = "include"
        });

        return await base.SendAsync(request, cancellationToken);
    }
}

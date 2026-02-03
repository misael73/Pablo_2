using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorApp1;
using BlazorApp1.Auth;
using BlazorApp1.Services;
using BlazorApp1.Handlers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register CookieHandler
builder.Services.AddScoped<CookieHandler>();

// Configure HttpClient for API calls with credentials support
builder.Services.AddHttpClient("API", client => 
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001");
})
.AddHttpMessageHandler<CookieHandler>();

// Register default HttpClient that uses the API client
builder.Services.AddScoped(sp => 
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return httpClientFactory.CreateClient("API");
});

// Add services
builder.Services.AddScoped<ApiService>();

// Add authentication
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<CustomAuthStateProvider>());

await builder.Build().RunAsync();
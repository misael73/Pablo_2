using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorApp1;
using BlazorApp1.Handlers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register CookieHandler as Transient - this is required because DelegatingHandler 
// instances must not be reused or cached by HttpMessageHandlerBuilder
builder.Services.AddTransient<CookieHandler>();

// Configure HttpClient with IHttpClientFactory pattern
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
})
.AddHttpMessageHandler<CookieHandler>();

// Register the named HttpClient as the default HttpClient service
builder.Services.AddScoped(sp => 
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));

// Register the custom authentication state provider
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
    sp.GetRequiredService<CustomAuthStateProvider>());

builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
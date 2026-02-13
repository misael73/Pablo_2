using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using BlazorApp1;
using BlazorApp1.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient to talk to the API
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5050/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Add API Service
builder.Services.AddScoped<ApiService>();

// Add Authentication
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());

await builder.Build().RunAsync();
using System.Net.Http.Json;
using BlazorApp1.Models;

namespace BlazorApp1.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;

    public ApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:5001";
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
    }

    // Authentication
    public async Task<LoginResponse> ValidateGoogleToken(string idToken)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("id_token", idToken)
        });

        var response = await _httpClient.PostAsync("/api/auth/google", content);
        return await response.Content.ReadFromJsonAsync<LoginResponse>() 
            ?? new LoginResponse { Success = false, Error = "Invalid response" };
    }

    public async Task<bool> Logout()
    {
        var response = await _httpClient.PostAsync("/api/auth/logout", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<UserModel?> GetCurrentUser()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<dynamic>("/api/auth/me");
            if (response?.success == true)
            {
                return response.usuario;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    // Reports
    public async Task<List<ReporteModel>> GetReportes(string? tipoDashboard = null)
    {
        var url = tipoDashboard != null 
            ? $"/api/reportes?tipoDashboard={tipoDashboard}" 
            : "/api/reportes";
        return await _httpClient.GetFromJsonAsync<List<ReporteModel>>(url) 
            ?? new List<ReporteModel>();
    }

    public async Task<ReporteModel?> GetReporte(int id)
    {
        return await _httpClient.GetFromJsonAsync<ReporteModel>($"/api/reportes/{id}");
    }

    public async Task<DashboardStats?> GetDashboardStats(string? tipoDashboard = null)
    {
        var url = tipoDashboard != null 
            ? $"/api/reportes/stats?tipoDashboard={tipoDashboard}" 
            : "/api/reportes/stats";
        return await _httpClient.GetFromJsonAsync<DashboardStats>(url);
    }

    public async Task<ReporteModel?> CreateReporte(CreateReporteModel model)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/reportes", model);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ReporteModel>();
        }
        return null;
    }

    // Catalogs
    public async Task<List<EdificioModel>> GetEdificios()
    {
        return await _httpClient.GetFromJsonAsync<List<EdificioModel>>("/api/edificios") 
            ?? new List<EdificioModel>();
    }

    public async Task<List<SalonModel>> GetSalones(int? edificioId = null)
    {
        var url = edificioId.HasValue 
            ? $"/api/salones?edificio_id={edificioId}" 
            : "/api/salones";
        return await _httpClient.GetFromJsonAsync<List<SalonModel>>(url) 
            ?? new List<SalonModel>();
    }

    public async Task<List<CategoriaModel>> GetCategorias()
    {
        return await _httpClient.GetFromJsonAsync<List<CategoriaModel>>("/api/categorias") 
            ?? new List<CategoriaModel>();
    }

    public async Task<List<PrioridadModel>> GetPrioridades()
    {
        // This endpoint doesn't exist yet, so we'll return hardcoded values for now
        return new List<PrioridadModel>
        {
            new PrioridadModel { Id = 1, Nombre = "Alta", Color = "#dc3545" },
            new PrioridadModel { Id = 2, Nombre = "Media", Color = "#ffc107" },
            new PrioridadModel { Id = 3, Nombre = "Baja", Color = "#28a745" }
        };
    }
}

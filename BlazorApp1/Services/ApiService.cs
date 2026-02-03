using System.Net.Http.Json;
using BlazorApp1.Models;

namespace BlazorApp1.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Authentication
    public async Task<ApiResponse<UsuarioDto>?> GoogleLoginAsync(string idToken)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Usuarios/google-login", new GoogleLoginDto { IdToken = idToken });
        return await response.Content.ReadFromJsonAsync<ApiResponse<UsuarioDto>>();
    }

    // Reports
    public async Task<ApiResponse<List<ReporteDto>>?> GetReportesAsync(int? usuarioId = null)
    {
        var url = usuarioId.HasValue ? $"api/Reportes/mis-reportes/{usuarioId}" : "api/Reportes";
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<ReporteDto>>>(url);
    }

    public async Task<ApiResponse<List<ReporteDto>>?> GetAllReportesAsync()
    {
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<ReporteDto>>>("api/Reportes");
    }

    public async Task<ApiResponse<List<ReporteDto>>?> GetReportesByDashboardAsync(string tipoDashboard)
    {
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<ReporteDto>>>($"api/Reportes/por-dashboard/{tipoDashboard}");
    }

    public async Task<ApiResponse<ReporteDto>?> GetReporteAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<ApiResponse<ReporteDto>>($"api/Reportes/{id}");
    }

    public async Task<ApiResponse<ReporteStatsDto>?> GetReporteStatsAsync(int? usuarioId = null)
    {
        var url = usuarioId.HasValue ? $"api/Reportes/estadisticas?usuarioId={usuarioId}" : "api/Reportes/estadisticas";
        return await _httpClient.GetFromJsonAsync<ApiResponse<ReporteStatsDto>>(url);
    }

    public async Task<ApiResponse<ReporteDto>?> CreateReporteAsync(CreateReporteDto reporte)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Reportes", reporte);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ReporteDto>>();
    }

    public async Task<ApiResponse<ReporteDto>?> UpdateReporteAsync(int id, UpdateReporteDto reporte)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/Reportes/{id}", reporte);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ReporteDto>>();
    }

    // Categories
    public async Task<ApiResponse<List<CategoriaDto>>?> GetCategoriasAsync(bool? activo = true)
    {
        var url = activo.HasValue ? $"api/Categorias?activo={activo}" : "api/Categorias";
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<CategoriaDto>>>(url);
    }

    public async Task<ApiResponse<CategoriaDto>?> CreateCategoriaAsync(CategoriaDto categoria)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Categorias", categoria);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CategoriaDto>>();
    }

    public async Task<ApiResponse<CategoriaDto>?> UpdateCategoriaAsync(int id, CategoriaDto categoria)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/Categorias/{id}", categoria);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CategoriaDto>>();
    }

    public async Task<ApiResponse<bool>?> DeleteCategoriaAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/Categorias/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    // Buildings
    public async Task<ApiResponse<List<EdificioDto>>?> GetEdificiosAsync(bool? activo = true)
    {
        var url = activo.HasValue ? $"api/Edificios?activo={activo}" : "api/Edificios";
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<EdificioDto>>>(url);
    }

    public async Task<ApiResponse<EdificioDto>?> CreateEdificioAsync(EdificioDto edificio)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Edificios", edificio);
        return await response.Content.ReadFromJsonAsync<ApiResponse<EdificioDto>>();
    }

    public async Task<ApiResponse<EdificioDto>?> UpdateEdificioAsync(int id, EdificioDto edificio)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/Edificios/{id}", edificio);
        return await response.Content.ReadFromJsonAsync<ApiResponse<EdificioDto>>();
    }

    public async Task<ApiResponse<bool>?> DeleteEdificioAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/Edificios/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    // Rooms
    public async Task<ApiResponse<List<SalonDto>>?> GetSalonesAsync(int? edificioId = null)
    {
        var url = edificioId.HasValue ? $"api/Salones/por-edificio/{edificioId}" : "api/Salones";
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<SalonDto>>>(url);
    }

    public async Task<ApiResponse<SalonDto>?> CreateSalonAsync(SalonDto salon)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Salones", salon);
        return await response.Content.ReadFromJsonAsync<ApiResponse<SalonDto>>();
    }

    public async Task<ApiResponse<SalonDto>?> UpdateSalonAsync(int id, SalonDto salon)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/Salones/{id}", salon);
        return await response.Content.ReadFromJsonAsync<ApiResponse<SalonDto>>();
    }

    public async Task<ApiResponse<bool>?> DeleteSalonAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/Salones/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    // Users
    public async Task<ApiResponse<List<UsuarioDto>>?> GetUsuariosAsync()
    {
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<UsuarioDto>>>("api/Usuarios");
    }

    public async Task<ApiResponse<List<UsuarioDto>>?> GetTecnicosAsync()
    {
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<UsuarioDto>>>("api/Usuarios/tecnicos");
    }

    public async Task<ApiResponse<UsuarioDto>?> UpdateUsuarioAsync(int id, UsuarioDto usuario)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/Usuarios/{id}", usuario);
        return await response.Content.ReadFromJsonAsync<ApiResponse<UsuarioDto>>();
    }

    // Estados y Prioridades
    public async Task<ApiResponse<List<EstadoPrioridadDto>>?> GetEstadosAsync()
    {
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<EstadoPrioridadDto>>>("api/Estados");
    }

    public async Task<ApiResponse<List<EstadoPrioridadDto>>?> GetPrioridadesAsync()
    {
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<EstadoPrioridadDto>>>("api/Prioridades");
    }

    // Comments
    public async Task<ApiResponse<List<ComentarioDto>>?> GetComentariosAsync(int reporteId)
    {
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<ComentarioDto>>>($"api/Comentarios/por-reporte/{reporteId}");
    }

    public async Task<ApiResponse<ComentarioDto>?> CreateComentarioAsync(CreateComentarioDto comentario)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Comentarios", comentario);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ComentarioDto>>();
    }
}

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

    // Categories
    public async Task<ApiResponse<List<CategoriaDto>>?> GetCategoriasAsync(bool? activo = true)
    {
        var url = activo.HasValue ? $"api/Categorias?activo={activo}" : "api/Categorias";
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<CategoriaDto>>>(url);
    }

    // Buildings
    public async Task<ApiResponse<List<EdificioDto>>?> GetEdificiosAsync(bool? activo = true)
    {
        var url = activo.HasValue ? $"api/Edificios?activo={activo}" : "api/Edificios";
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<EdificioDto>>>(url);
    }

    // Rooms
    public async Task<ApiResponse<List<SalonDto>>?> GetSalonesAsync(int? edificioId = null)
    {
        var url = edificioId.HasValue ? $"api/Salones/por-edificio/{edificioId}" : "api/Salones?activo=true";
        return await _httpClient.GetFromJsonAsync<ApiResponse<List<SalonDto>>>(url);
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

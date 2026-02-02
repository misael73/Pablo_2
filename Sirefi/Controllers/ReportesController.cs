using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Sirefi.DTOs;
using Sirefi.Services;

namespace Sirefi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportesController : BaseApiController
{
    private readonly IReporteService _reporteService;
    private readonly IFileService _fileService;

    public ReportesController(IReporteService reporteService, IFileService fileService)
    {
        _reporteService = reporteService;
        _fileService = fileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetReportes([FromQuery] string? tipoDashboard = null)
    {
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();

        IEnumerable<Models.Reporte> reportes;

        if (!string.IsNullOrEmpty(tipoDashboard))
        {
            reportes = await _reporteService.GetReportesByCategory(tipoDashboard);
        }
        else
        {
            reportes = await _reporteService.GetAllReportes(userId, role);
        }

        return Ok(reportes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReporte(int id)
    {
        var reporte = await _reporteService.GetReporteById(id);

        if (reporte == null)
        {
            return NotFound(new { error = "Reporte no encontrado" });
        }

        // Check if user has permission to view this reporte
        var userId = GetCurrentUserId();
        var role = GetCurrentUserRole();

        if (role == Constants.Roles.Reportante && reporte.IdReportante != userId)
        {
            return Forbid();
        }

        return Ok(reporte);
    }

    [HttpGet("folio/{folio}")]
    public async Task<IActionResult> GetReporteByFolio(string folio)
    {
        var reporte = await _reporteService.GetReporteByFolio(folio);

        if (reporte == null)
        {
            return NotFound(new { error = "Reporte no encontrado" });
        }

        return Ok(reporte);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReporte([FromForm] CreateReporteDto dto, [FromForm] IFormFile? archivo)
    {
        try
        {
            var userId = GetCurrentUserId();
            var reporte = await _reporteService.CreateReporte(dto, userId);

            // Handle file upload if present
            if (archivo != null)
            {
                await _fileService.SaveFile(archivo, reporte.Id, userId);
            }

            return CreatedAtAction(nameof(GetReporte), new { id = reporte.Id }, reporte);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOrTechnician")]
    public async Task<IActionResult> UpdateReporte(int id, [FromBody] UpdateReporteDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var reporte = await _reporteService.UpdateReporte(id, dto, userId);

            return Ok(reporte);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteReporte(int id)
    {
        var result = await _reporteService.DeleteReporte(id);

        if (!result)
        {
            return NotFound(new { error = "Reporte no encontrado" });
        }

        return Ok(new { success = true, message = "Reporte eliminado correctamente" });
    }

    [HttpPost("{id}/upload")]
    public async Task<IActionResult> UploadFile(int id, [FromForm] IFormFile archivo)
    {
        try
        {
            var reporte = await _reporteService.GetReporteById(id);
            if (reporte == null)
            {
                return NotFound(new { error = "Reporte no encontrado" });
            }

            // Check permission
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (role == Constants.Roles.Reportante && reporte.IdReportante != userId)
            {
                return Forbid();
            }

            var fileName = await _fileService.SaveFile(archivo, id, userId);

            return Ok(new { success = true, fileName = fileName });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("stats")]
    [Authorize(Policy = "AdminOrTechnician")]
    public async Task<IActionResult> GetDashboardStats([FromQuery] string? tipoDashboard = null)
    {
        var stats = await _reporteService.GetDashboardStats(tipoDashboard);
        return Ok(stats);
    }
}

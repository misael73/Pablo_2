using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Sirefi.DTOs;
using Sirefi.Services;

namespace Sirefi.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class EdificiosController : BaseApiController
{
    private readonly IInfrastructureService _infrastructureService;

    public EdificiosController(IInfrastructureService infrastructureService)
    {
        _infrastructureService = infrastructureService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetEdificios([FromQuery] string? action = null)
    {
        if (action == "list")
        {
            var edificios = await _infrastructureService.GetAllEdificios();
            return Ok(edificios);
        }

        var activeEdificios = await _infrastructureService.GetActiveEdificios();
        return Ok(activeEdificios);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEdificio(int id)
    {
        var edificio = await _infrastructureService.GetEdificioById(id);

        if (edificio == null)
        {
            return NotFound(new { error = "Edificio no encontrado" });
        }

        return Ok(edificio);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEdificio([FromBody] CreateEdificioDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var edificio = await _infrastructureService.CreateEdificio(dto, userId);

            return CreatedAtAction(nameof(GetEdificio), new { id = edificio.Id }, edificio);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEdificio(int id, [FromBody] UpdateEdificioDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var edificio = await _infrastructureService.UpdateEdificio(id, dto, userId);

            return Ok(edificio);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEdificio(int id)
    {
        try
        {
            var result = await _infrastructureService.DeleteEdificio(id);

            if (!result)
            {
                return NotFound(new { error = "Edificio no encontrado" });
            }

            return Ok(new { success = true, message = "Edificio eliminado correctamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

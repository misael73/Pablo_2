using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Sirefi.DTOs;
using Sirefi.Services;

namespace Sirefi.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class SalonesController : BaseApiController
{
    private readonly IInfrastructureService _infrastructureService;

    public SalonesController(IInfrastructureService infrastructureService)
    {
        _infrastructureService = infrastructureService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetSalones([FromQuery] int? edificio_id = null)
    {
        if (edificio_id.HasValue)
        {
            var salones = await _infrastructureService.GetActiveSalonesByEdificio(edificio_id.Value);
            return Ok(salones);
        }

        var allSalones = await _infrastructureService.GetAllSalones();
        return Ok(allSalones);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSalon(int id)
    {
        var salon = await _infrastructureService.GetSalonById(id);

        if (salon == null)
        {
            return NotFound(new { error = "Salón no encontrado" });
        }

        return Ok(salon);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSalon([FromBody] CreateSalonDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var salon = await _infrastructureService.CreateSalon(dto, userId);

            return CreatedAtAction(nameof(GetSalon), new { id = salon.Id }, salon);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSalon(int id, [FromBody] UpdateSalonDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var salon = await _infrastructureService.UpdateSalon(id, dto, userId);

            return Ok(salon);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSalon(int id)
    {
        try
        {
            var result = await _infrastructureService.DeleteSalon(id);

            if (!result)
            {
                return NotFound(new { error = "Salón no encontrado" });
            }

            return Ok(new { success = true, message = "Salón eliminado correctamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

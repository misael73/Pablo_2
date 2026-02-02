using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sirefi.DTOs;
using Sirefi.Services;

namespace Sirefi.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriasController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategorias()
    {
        var categorias = await _categoryService.GetActiveCategorias();
        return Ok(categorias);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllCategorias()
    {
        var categorias = await _categoryService.GetAllCategorias();
        return Ok(categorias);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoria(int id)
    {
        var categoria = await _categoryService.GetCategoriaById(id);

        if (categoria == null)
        {
            return NotFound(new { error = "Categoría no encontrada" });
        }

        return Ok(categoria);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategoria([FromBody] CreateCategoriaDto dto)
    {
        try
        {
            var categoria = await _categoryService.CreateCategoria(dto);
            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoria);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategoria(int id, [FromBody] UpdateCategoriaDto dto)
    {
        try
        {
            var categoria = await _categoryService.UpdateCategoria(id, dto);
            return Ok(categoria);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/toggle")]
    public async Task<IActionResult> ToggleCategoria(int id)
    {
        var result = await _categoryService.ToggleCategoria(id);

        if (!result)
        {
            return NotFound(new { error = "Categoría no encontrada" });
        }

        return Ok(new { success = true, message = "Estado de categoría actualizado" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategoria(int id)
    {
        try
        {
            var result = await _categoryService.DeleteCategoria(id);

            if (!result)
            {
                return NotFound(new { error = "Categoría no encontrada" });
            }

            return Ok(new { success = true, message = "Categoría eliminada correctamente" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

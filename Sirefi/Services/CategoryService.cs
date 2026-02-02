using Microsoft.EntityFrameworkCore;
using Sirefi.Data;
using Sirefi.DTOs;
using Sirefi.Models;

namespace Sirefi.Services;

public class CategoryService : ICategoryService
{
    private readonly FormsDbContext _context;

    public CategoryService(FormsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Categoria>> GetAllCategorias()
    {
        return await _context.Categorias
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Categoria>> GetActiveCategorias()
    {
        return await _context.Categorias
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<Categoria?> GetCategoriaById(int id)
    {
        return await _context.Categorias.FindAsync(id);
    }

    public async Task<Categoria> CreateCategoria(CreateCategoriaDto dto)
    {
        // Check if name already exists
        var exists = await _context.Categorias.AnyAsync(c => c.Nombre == dto.Nombre);
        if (exists)
        {
            throw new Exception("Ya existe una categoría con ese nombre");
        }

        var categoria = new Categoria
        {
            Nombre = dto.Nombre,
            TipoDashboard = dto.TipoDashboard,
            Descripcion = dto.Descripcion,
            Icono = dto.Icono,
            Color = dto.Color,
            Activo = dto.Activo,
            FechaCreacion = DateTime.Now
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return categoria;
    }

    public async Task<Categoria> UpdateCategoria(int id, UpdateCategoriaDto dto)
    {
        var categoria = await GetCategoriaById(id);
        if (categoria == null)
        {
            throw new Exception("Categoría no encontrada");
        }

        // Check if new name already exists (excluding current categoria)
        var exists = await _context.Categorias
            .AnyAsync(c => c.Nombre == dto.Nombre && c.Id != id);
        if (exists)
        {
            throw new Exception("Ya existe una categoría con ese nombre");
        }

        categoria.Nombre = dto.Nombre;
        categoria.TipoDashboard = dto.TipoDashboard;
        categoria.Descripcion = dto.Descripcion;
        categoria.Icono = dto.Icono;
        categoria.Color = dto.Color;
        categoria.Activo = dto.Activo;

        await _context.SaveChangesAsync();

        return categoria;
    }

    public async Task<bool> ToggleCategoria(int id)
    {
        var categoria = await GetCategoriaById(id);
        if (categoria == null)
        {
            return false;
        }

        categoria.Activo = !categoria.Activo;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteCategoria(int id)
    {
        var categoria = await GetCategoriaById(id);
        if (categoria == null)
        {
            return false;
        }

        // Check if has related reportes
        var hasReportes = await _context.Reportes
            .AnyAsync(r => r.IdCategoria == id && !r.Eliminado);
        if (hasReportes)
        {
            throw new Exception("No se puede eliminar la categoría porque tiene reportes asociados");
        }

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();

        return true;
    }
}

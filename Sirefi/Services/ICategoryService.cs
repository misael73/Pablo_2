using Sirefi.DTOs;
using Sirefi.Models;

namespace Sirefi.Services;

public interface ICategoryService
{
    Task<IEnumerable<Categoria>> GetAllCategorias();
    Task<IEnumerable<Categoria>> GetActiveCategorias();
    Task<Categoria?> GetCategoriaById(int id);
    Task<Categoria> CreateCategoria(CreateCategoriaDto dto);
    Task<Categoria> UpdateCategoria(int id, UpdateCategoriaDto dto);
    Task<bool> ToggleCategoria(int id);
    Task<bool> DeleteCategoria(int id);
}

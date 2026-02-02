using Sirefi.DTOs;
using Sirefi.Models;

namespace Sirefi.Services;

public interface IInfrastructureService
{
    // Edificios
    Task<IEnumerable<Edificio>> GetAllEdificios();
    Task<IEnumerable<Edificio>> GetActiveEdificios();
    Task<Edificio?> GetEdificioById(int id);
    Task<Edificio> CreateEdificio(CreateEdificioDto dto, int userId);
    Task<Edificio> UpdateEdificio(int id, UpdateEdificioDto dto, int userId);
    Task<bool> DeleteEdificio(int id);
    
    // Salones
    Task<IEnumerable<Salone>> GetAllSalones();
    Task<IEnumerable<Salone>> GetSalonesByEdificio(int edificioId);
    Task<IEnumerable<Salone>> GetActiveSalonesByEdificio(int edificioId);
    Task<Salone?> GetSalonById(int id);
    Task<Salone> CreateSalon(CreateSalonDto dto, int userId);
    Task<Salone> UpdateSalon(int id, UpdateSalonDto dto, int userId);
    Task<bool> DeleteSalon(int id);
}

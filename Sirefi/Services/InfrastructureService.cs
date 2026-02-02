using Microsoft.EntityFrameworkCore;
using Sirefi.Data;
using Sirefi.DTOs;
using Sirefi.Models;

namespace Sirefi.Services;

public class InfrastructureService : IInfrastructureService
{
    private readonly FormsDbContext _context;

    public InfrastructureService(FormsDbContext context)
    {
        _context = context;
    }

    // Edificios
    public async Task<IEnumerable<Edificio>> GetAllEdificios()
    {
        return await _context.Edificios
            .OrderBy(e => e.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Edificio>> GetActiveEdificios()
    {
        return await _context.Edificios
            .Where(e => e.Activo)
            .OrderBy(e => e.Nombre)
            .ToListAsync();
    }

    public async Task<Edificio?> GetEdificioById(int id)
    {
        return await _context.Edificios
            .Include(e => e.Salones)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Edificio> CreateEdificio(CreateEdificioDto dto, int userId)
    {
        // Check if name already exists
        var exists = await _context.Edificios.AnyAsync(e => e.Nombre == dto.Nombre);
        if (exists)
        {
            throw new Exception("Ya existe un edificio con ese nombre");
        }

        var edificio = new Edificio
        {
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Ubicacion = dto.Ubicacion,
            Pisos = dto.Pisos,
            Activo = dto.Activo,
            FechaCreacion = DateTime.Now,
            CreadoPor = userId
        };

        _context.Edificios.Add(edificio);
        await _context.SaveChangesAsync();

        return edificio;
    }

    public async Task<Edificio> UpdateEdificio(int id, UpdateEdificioDto dto, int userId)
    {
        var edificio = await GetEdificioById(id);
        if (edificio == null)
        {
            throw new Exception("Edificio no encontrado");
        }

        // Check if new name already exists (excluding current edificio)
        var exists = await _context.Edificios
            .AnyAsync(e => e.Nombre == dto.Nombre && e.Id != id);
        if (exists)
        {
            throw new Exception("Ya existe un edificio con ese nombre");
        }

        edificio.Codigo = dto.Codigo;
        edificio.Nombre = dto.Nombre;
        edificio.Descripcion = dto.Descripcion;
        edificio.Ubicacion = dto.Ubicacion;
        edificio.Pisos = dto.Pisos;
        edificio.Activo = dto.Activo;
        edificio.FechaActualizacion = DateTime.Now;
        edificio.ActualizadoPor = userId;

        await _context.SaveChangesAsync();

        return edificio;
    }

    public async Task<bool> DeleteEdificio(int id)
    {
        var edificio = await GetEdificioById(id);
        if (edificio == null)
        {
            return false;
        }

        // Check if has related salones
        var hasSalones = await _context.Salones.AnyAsync(s => s.IdEdificio == id);
        if (hasSalones)
        {
            throw new Exception("No se puede eliminar el edificio porque tiene salones asociados");
        }

        _context.Edificios.Remove(edificio);
        await _context.SaveChangesAsync();

        return true;
    }

    // Salones
    public async Task<IEnumerable<Salone>> GetAllSalones()
    {
        return await _context.Salones
            .Include(s => s.IdEdificioNavigation)
            .OrderBy(s => s.IdEdificioNavigation.Nombre)
            .ThenBy(s => s.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Salone>> GetSalonesByEdificio(int edificioId)
    {
        return await _context.Salones
            .Where(s => s.IdEdificio == edificioId)
            .OrderBy(s => s.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Salone>> GetActiveSalonesByEdificio(int edificioId)
    {
        return await _context.Salones
            .Where(s => s.IdEdificio == edificioId && s.Activo)
            .OrderBy(s => s.Nombre)
            .ToListAsync();
    }

    public async Task<Salone?> GetSalonById(int id)
    {
        return await _context.Salones
            .Include(s => s.IdEdificioNavigation)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Salone> CreateSalon(CreateSalonDto dto, int userId)
    {
        // Check if name already exists in the same edificio
        var exists = await _context.Salones
            .AnyAsync(s => s.IdEdificio == dto.IdEdificio && s.Nombre == dto.Nombre);
        if (exists)
        {
            throw new Exception("Ya existe un salón con ese nombre en este edificio");
        }

        var salon = new Salone
        {
            IdEdificio = dto.IdEdificio,
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Piso = dto.Piso,
            TipoEspacio = dto.TipoEspacio,
            Capacidad = dto.Capacidad,
            Activo = dto.Activo,
            FechaCreacion = DateTime.Now,
            CreadoPor = userId
        };

        _context.Salones.Add(salon);
        await _context.SaveChangesAsync();

        return salon;
    }

    public async Task<Salone> UpdateSalon(int id, UpdateSalonDto dto, int userId)
    {
        var salon = await GetSalonById(id);
        if (salon == null)
        {
            throw new Exception("Salón no encontrado");
        }

        // Check if new name already exists in the same edificio (excluding current salon)
        var exists = await _context.Salones
            .AnyAsync(s => s.IdEdificio == dto.IdEdificio && s.Nombre == dto.Nombre && s.Id != id);
        if (exists)
        {
            throw new Exception("Ya existe un salón con ese nombre en este edificio");
        }

        salon.IdEdificio = dto.IdEdificio;
        salon.Codigo = dto.Codigo;
        salon.Nombre = dto.Nombre;
        salon.Descripcion = dto.Descripcion;
        salon.Piso = dto.Piso;
        salon.TipoEspacio = dto.TipoEspacio;
        salon.Capacidad = dto.Capacidad;
        salon.Activo = dto.Activo;
        salon.FechaActualizacion = DateTime.Now;
        salon.ActualizadoPor = userId;

        await _context.SaveChangesAsync();

        return salon;
    }

    public async Task<bool> DeleteSalon(int id)
    {
        var salon = await GetSalonById(id);
        if (salon == null)
        {
            return false;
        }

        // Check if has related reportes
        var hasReportes = await _context.Reportes
            .AnyAsync(r => r.IdSalon == id && !r.Eliminado);
        if (hasReportes)
        {
            throw new Exception("No se puede eliminar el salón porque tiene reportes asociados");
        }

        _context.Salones.Remove(salon);
        await _context.SaveChangesAsync();

        return true;
    }
}

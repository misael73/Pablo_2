using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sirefi.Data;
using Sirefi.Models;

namespace Sirefi.Services;

public class FileService : IFileService
{
    private readonly FormsDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileService> _logger;
    private readonly string _uploadPath;
    private readonly long _maxFileSize;
    private readonly string[] _allowedExtensions;

    public FileService(FormsDbContext context, IConfiguration configuration, ILogger<FileService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        
        _uploadPath = _configuration["FileUpload:UploadPath"] ?? "uploads";
        
        if (!long.TryParse(_configuration["FileUpload:MaxFileSize"], out _maxFileSize))
        {
            _maxFileSize = 2097152; // Default 2MB
        }
        
        _allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>() 
            ?? new[] { "jpg", "jpeg", "png", "pdf" };

        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public bool ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return false;
        }

        if (file.Length > _maxFileSize)
        {
            throw new Exception($"El archivo excede el tamaño máximo permitido de {_maxFileSize / 1024 / 1024}MB");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant().TrimStart('.');
        if (!_allowedExtensions.Contains(extension))
        {
            throw new Exception($"Tipo de archivo no permitido. Extensiones permitidas: {string.Join(", ", _allowedExtensions)}");
        }

        return true;
    }

    public async Task<string> SaveFile(IFormFile file, int reporteId, int userId)
    {
        if (!ValidateFile(file))
        {
            throw new Exception("Archivo inválido");
        }

        var extension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_uploadPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Save file info to database
        var archivo = new Archivo
        {
            IdReporte = reporteId,
            IdUsuario = userId,
            NombreOriginal = file.FileName,
            NombreArchivo = uniqueFileName,
            Ruta = filePath,
            TipoMime = file.ContentType,
            TamanoBytes = file.Length,
            FechaSubida = DateTime.Now,
            Eliminado = false
        };

        _context.Archivos.Add(archivo);
        await _context.SaveChangesAsync();

        return uniqueFileName;
    }

    public async Task<(byte[] fileBytes, string contentType, string fileName)?> GetFile(string fileName)
    {
        var archivo = await _context.Archivos
            .FirstOrDefaultAsync(a => a.NombreArchivo == fileName && !a.Eliminado);

        if (archivo == null)
        {
            return null;
        }

        var filePath = Path.Combine(_uploadPath, fileName);
        if (!File.Exists(filePath))
        {
            return null;
        }

        var fileBytes = await File.ReadAllBytesAsync(filePath);
        return (fileBytes, archivo.TipoMime ?? "application/octet-stream", archivo.NombreOriginal ?? fileName);
    }

    public async Task<bool> DeleteFile(string fileName)
    {
        var archivo = await _context.Archivos
            .FirstOrDefaultAsync(a => a.NombreArchivo == fileName);

        if (archivo == null)
        {
            return false;
        }

        archivo.Eliminado = true;
        await _context.SaveChangesAsync();

        // Try to delete physical file, but don't fail if it doesn't exist
        try
        {
            var filePath = Path.Combine(_uploadPath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the operation
            // The file is marked as deleted in the database
            _logger.LogWarning(ex, "Could not delete physical file {FileName}", fileName);
        }

        return true;
    }

    public string GetUploadPath()
    {
        return _uploadPath;
    }
}

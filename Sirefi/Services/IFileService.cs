using Microsoft.AspNetCore.Http;

namespace Sirefi.Services;

public interface IFileService
{
    Task<string> SaveFile(IFormFile file, int reporteId, int userId);
    Task<(byte[] fileBytes, string contentType, string fileName)?> GetFile(string fileName);
    Task<bool> DeleteFile(string fileName);
    bool ValidateFile(IFormFile file);
    string GetUploadPath();
}

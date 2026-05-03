using Microsoft.AspNetCore.Http;

namespace Partivex.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subfolder);

    void DeleteFile(string relativePath);
}

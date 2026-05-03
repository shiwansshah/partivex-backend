using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Partivex.Application.Interfaces;

namespace Partivex.Infrastructure.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB

    private readonly string _webRootPath;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _webRootPath = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subfolder)
    {
        if (file.Length == 0)
        {
            throw new ArgumentException("File is empty.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException("File size must not exceed 2 MB.");
        }

        var extension = Path.GetExtension(file.FileName);

        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Only JPG, PNG, and WebP images are allowed.");
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var folderPath = Path.Combine(_webRootPath, "uploads", subfolder);

        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{subfolder}/{fileName}";
    }

    public void DeleteFile(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;

        var fullPath = Path.Combine(_webRootPath, relativePath.TrimStart('/'));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}

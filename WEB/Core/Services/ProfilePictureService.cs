using Microsoft.AspNetCore.Components.Forms;

namespace WEB.Core.Services;

public class ProfilePictureService
{
    private readonly IWebHostEnvironment _env;
    private const string UploadFolder = "uploads/avatars";

    public ProfilePictureService(IWebHostEnvironment env) => _env = env;

    public async Task<string> SaveAsync(IBrowserFile file, string userId)
    {
        var uploadPath = Path.Combine(_env.WebRootPath, UploadFolder);
        Directory.CreateDirectory(uploadPath);

        var extension = Path.GetExtension(file.Name) ?? ".jpg";
        var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);

        await using var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
        await using var fs = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fs);
        
        var oldFiles = Directory.GetFiles(uploadPath, $"{userId}_*");
        foreach (var oldFile in oldFiles)
        {
            if (oldFile != filePath)
                File.Delete(oldFile);
        }

        return $"/{UploadFolder}/{fileName}";
    }
    
    public async Task<string> SaveFromLocalFileAsync(string tempFilePath, string originalFileName, string userId)
    {
        var uploadPath = Path.Combine(_env.WebRootPath, UploadFolder);
        Directory.CreateDirectory(uploadPath);
        
        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrEmpty(extension))
            extension = ".jpg";

        var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var destPath = Path.Combine(uploadPath, fileName);

        await using (var sourceStream = File.OpenRead(tempFilePath))
        await using (var destStream = File.Create(destPath))
        {
            await sourceStream.CopyToAsync(destStream);
        }

    
        try { File.Delete(tempFilePath); } catch { /* ignorar si falla, no es crítico */ }

     
        var oldFiles = Directory.GetFiles(uploadPath, $"{userId}_*");
        foreach (var oldFile in oldFiles)
        {
            if (oldFile != destPath)
                File.Delete(oldFile);
        }

        return $"/{UploadFolder}/{fileName}";
    }
}
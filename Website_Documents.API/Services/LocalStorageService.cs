using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Services;

public class LocalStorageService : IStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(IConfiguration configuration, ILogger<LocalStorageService> logger)
    {
        _logger = logger;
        
        _basePath = configuration["Storage:Local:BasePath"] ?? "wwwroot/uploads";
        _baseUrl = configuration["Storage:Local:BaseUrl"] ?? "/uploads";
        
        // Ensure directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
            _logger.LogInformation("Created uploads directory: {Path}", _basePath);
        }
        
        _logger.LogInformation("Local Storage initialized at: {Path}", _basePath);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null)
    {
        try
        {
            // Generate unique file key
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var extension = Path.GetExtension(fileName);
            var cleanFileName = Path.GetFileNameWithoutExtension(fileName);
            
            // Sanitize filename
            cleanFileName = string.Join("_", cleanFileName.Split(Path.GetInvalidFileNameChars()));
            
            // Determine target folder
            var targetFolder = string.IsNullOrEmpty(folder) 
                ? _basePath 
                : Path.Combine(_basePath, folder);
            
            // Create subfolder if needed
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }
            
            var fileKey = $"{timestamp}_{uniqueId}_{cleanFileName}{extension}";
            var fullPath = Path.Combine(targetFolder, fileKey);
            
            // Reset stream position if needed
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }
            
            // Save file
            using var fileStreamOut = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fileStreamOut);
            
            // Return URL (relative to wwwroot)
            var relativePath = string.IsNullOrEmpty(folder) 
                ? $"{_baseUrl}/{fileKey}"
                : $"{_baseUrl}/{folder}/{fileKey}";
            
            _logger.LogInformation("File uploaded successfully: {FileKey}", relativePath);
            return relativePath.Replace("\\", "/");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> UploadFileAsync(byte[] fileData, string fileName, string contentType, string? folder = null)
    {
        using var stream = new MemoryStream(fileData);
        return await UploadFileAsync(stream, fileName, contentType, folder);
    }

    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var fileKey = ExtractFileKey(fileUrl);
            if (string.IsNullOrEmpty(fileKey))
            {
                return Task.FromResult(false);
            }

            var fullPath = Path.Combine(_basePath, fileKey);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted successfully: {FileKey}", fileKey);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FileUrl}", fileUrl);
            return Task.FromResult(false);
        }
    }

    public Task<string> GetPresignedUrlAsync(string fileKey, int expirationMinutes = 60)
    {
        // For local storage, just return the direct URL
        var url = fileKey.StartsWith("/") ? fileKey : $"/{fileKey}";
        return Task.FromResult(url);
    }

    public Task<bool> FileExistsAsync(string fileKey)
    {
        try
        {
            var fullPath = Path.Combine(_basePath, fileKey);
            return Task.FromResult(File.Exists(fullPath));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private string? ExtractFileKey(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl))
            return null;

        try
        {
            // Handle URLs like /uploads/folder/file.jpg or uploads/folder/file.jpg
            var path = fileUrl;
            
            if (path.Contains(_baseUrl))
            {
                var index = path.IndexOf(_baseUrl);
                path = path.Substring(index + _baseUrl.Length).TrimStart('/');
            }
            else if (path.StartsWith("/"))
            {
                path = path.TrimStart('/');
            }
            
            return path.Split('?')[0]; // Remove query string if present
        }
        catch
        {
            return null;
        }
    }
}

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Services;

public class NeonStorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _baseUrl;
    private readonly ILogger<NeonStorageService> _logger;
    private readonly bool _isConfigured;

    public NeonStorageService(IConfiguration configuration, ILogger<NeonStorageService> logger)
    {
        _logger = logger;
        
        // Get Neon Storage configuration
        var endpoint = configuration["NeonStorage:Endpoint"];
        var accessKey = configuration["NeonStorage:AccessKey"];
        var secretKey = configuration["NeonStorage:SecretKey"];
        _bucketName = configuration["NeonStorage:BucketName"] ?? "documents";
        _baseUrl = configuration["NeonStorage:BaseUrl"] ?? "";

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
        {
            _logger.LogWarning("Neon Storage credentials not configured. File uploads will be disabled.");
            _isConfigured = false;
            return;
        }

        _isConfigured = true;

        var s3Config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true, // Required for S3-compatible services
            UseHttp = endpoint.StartsWith("http://")
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);
        _logger.LogInformation("Neon Storage initialized with endpoint: {Endpoint}, bucket: {Bucket}", endpoint, _bucketName);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null)
    {
        if (!_isConfigured)
        {
            throw new InvalidOperationException("Neon Storage is not configured. Please set NeonStorage credentials in configuration.");
        }

        try
        {
            // Generate unique file key
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var extension = Path.GetExtension(fileName);
            var cleanFileName = Path.GetFileNameWithoutExtension(fileName);
            
            // Sanitize filename
            cleanFileName = string.Join("_", cleanFileName.Split(Path.GetInvalidFileNameChars()));
            
            var fileKey = string.IsNullOrEmpty(folder) 
                ? $"{timestamp}_{uniqueId}_{cleanFileName}{extension}"
                : $"{folder.TrimEnd('/')}/{timestamp}_{uniqueId}_{cleanFileName}{extension}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileKey,
                InputStream = fileStream,
                ContentType = contentType
            };

            await _s3Client.PutObjectAsync(request);

            var fileUrl = string.IsNullOrEmpty(_baseUrl) 
                ? $"https://{_bucketName}.s3.amazonaws.com/{fileKey}"
                : $"{_baseUrl.TrimEnd('/')}/{fileKey}";

            _logger.LogInformation("File uploaded successfully: {FileKey}", fileKey);
            return fileUrl;
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

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        if (!_isConfigured)
        {
            return false;
        }

        try
        {
            // Extract file key from URL
            var fileKey = ExtractFileKey(fileUrl);
            if (string.IsNullOrEmpty(fileKey))
            {
                return false;
            }

            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fileKey
            };

            await _s3Client.DeleteObjectAsync(request);
            _logger.LogInformation("File deleted successfully: {FileKey}", fileKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FileUrl}", fileUrl);
            return false;
        }
    }

    public async Task<string> GetPresignedUrlAsync(string fileKey, int expirationMinutes = 60)
    {
        if (!_isConfigured)
        {
            throw new InvalidOperationException("Neon Storage is not configured.");
        }

        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileKey,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            var url = await _s3Client.GetPreSignedURLAsync(request);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate presigned URL for: {FileKey}", fileKey);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string fileKey)
    {
        if (!_isConfigured)
        {
            return false;
        }

        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = fileKey
            };

            await _s3Client.GetObjectMetadataAsync(request);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check file existence: {FileKey}", fileKey);
            return false;
        }
    }

    private string? ExtractFileKey(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl))
            return null;

        try
        {
            // Handle various URL formats
            // https://bucket.s3.amazonaws.com/folder/file.jpg
            // https://endpoint/bucket/folder/file.jpg
            // https://endpoint/bucket/folder/file.jpg?query=params

            var uri = new Uri(fileUrl);
            var path = uri.AbsolutePath.TrimStart('/');

            // If path starts with bucket name, remove it
            if (path.StartsWith(_bucketName + "/"))
            {
                path = path.Substring(_bucketName.Length + 1);
            }

            return path.Split('?')[0]; // Remove query string if present
        }
        catch
        {
            return null;
        }
    }
}

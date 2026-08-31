namespace Website_Documents.Service.Interfaces;

public interface IStorageService
{
    /// <summary>
    /// Upload a file to storage
    /// </summary>
    /// <param name="fileStream">File content as stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <param name="folder">Optional folder path within bucket</param>
    /// <returns>URL of the uploaded file</returns>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null);

    /// <summary>
    /// Upload a file from byte array
    /// </summary>
    Task<string> UploadFileAsync(byte[] fileData, string fileName, string contentType, string? folder = null);

    /// <summary>
    /// Delete a file from storage
    /// </summary>
    Task<bool> DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Generate a presigned URL for temporary access
    /// </summary>
    Task<string> GetPresignedUrlAsync(string fileKey, int expirationMinutes = 60);

    /// <summary>
    /// Check if a file exists
    /// </summary>
    Task<bool> FileExistsAsync(string fileKey);
}

public class StorageUploadResult
{
    public bool Success { get; set; }
    public string? FileUrl { get; set; }
    public string? FileKey { get; set; }
    public string? ErrorMessage { get; set; }
}

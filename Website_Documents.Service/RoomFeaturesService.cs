using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class RoomMusicService : IRoomMusicService
{
    private readonly BookstoreDbContext _context;
    private readonly string _uploadFolder;

    public RoomMusicService(BookstoreDbContext context)
    {
        _context = context;
        _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "music");
        Directory.CreateDirectory(_uploadFolder);
    }

    public async Task<IEnumerable<RoomMusicTrack>> GetTracksAsync(long spaceId)
    {
        return await _context.RoomMusicTracks
            .Where(t => t.SpaceId == spaceId)
            .Include(t => t.Uploader)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<RoomMusicTrack> AddTrackAsync(long spaceId, long userId, string title, string? artist, string sourceType, string? filePath, string? externalUrl, int durationSeconds)
    {
        var track = new RoomMusicTrack
        {
            SpaceId = spaceId,
            Title = title,
            Artist = artist,
            SourceType = sourceType,
            FilePath = filePath,
            ExternalUrl = externalUrl,
            DurationSeconds = durationSeconds,
            UploadedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.RoomMusicTracks.Add(track);
        await _context.SaveChangesAsync();

        return track;
    }

    public async Task<bool> DeleteTrackAsync(long trackId, long userId)
    {
        var track = await _context.RoomMusicTracks
            .FirstOrDefaultAsync(t => t.Id == trackId);
        
        if (track == null) return false;

        // Check if user is the uploader
        if (track.UploadedBy != userId) return false;

        // Delete physical file if exists
        if (!string.IsNullOrEmpty(track.FilePath))
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", track.FilePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        _context.RoomMusicTracks.Remove(track);
        var result = await _context.SaveChangesAsync();

        return result > 0;
    }

    public async Task<string> UploadMusicFileAsync(long userId, byte[] fileData, string fileName)
    {
        var userFolder = Path.Combine(_uploadFolder, userId.ToString());
        Directory.CreateDirectory(userFolder);

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(userFolder, uniqueFileName);

        await File.WriteAllBytesAsync(filePath, fileData);

        return $"/uploads/music/{userId}/{uniqueFileName}";
    }
}

public class RoomFileService : IRoomFileService
{
    private readonly BookstoreDbContext _context;
    private readonly string _uploadFolder;

    public RoomFileService(BookstoreDbContext context)
    {
        _context = context;
        _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "files");
        Directory.CreateDirectory(_uploadFolder);
    }

    public async Task<IEnumerable<RoomSharedFile>> GetFilesAsync(long spaceId)
    {
        return await _context.RoomSharedFiles
            .Where(f => f.SpaceId == spaceId)
            .Include(f => f.Uploader)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<RoomSharedFile> UploadFileAsync(long spaceId, long userId, byte[] fileData, string fileName, string contentType)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var userFolder = Path.Combine(_uploadFolder, userId.ToString(), spaceId.ToString());
        Directory.CreateDirectory(userFolder);

        var filePath = Path.Combine(userFolder, uniqueFileName);
        await File.WriteAllBytesAsync(filePath, fileData);

        var fileType = GetFileType(contentType);
        var relativePath = $"/uploads/files/{userId}/{spaceId}/{uniqueFileName}";

        var file = new RoomSharedFile
        {
            SpaceId = spaceId,
            FileName = uniqueFileName,
            OriginalName = fileName,
            FilePath = relativePath,
            FileSize = fileData.Length,
            ContentType = contentType,
            FileType = fileType,
            UploadedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.RoomSharedFiles.Add(file);
        await _context.SaveChangesAsync();

        return file;
    }

    public async Task<(byte[] fileData, string fileName, string contentType)?> DownloadFileAsync(long fileId, long userId)
    {
        var file = await _context.RoomSharedFiles.FindAsync(fileId);
        if (file == null) return null;

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));
        if (!File.Exists(fullPath)) return null;

        var fileData = await File.ReadAllBytesAsync(fullPath);
        return (fileData, file.OriginalName, file.ContentType);
    }

    public async Task<bool> DeleteFileAsync(long fileId, long userId)
    {
        var file = await _context.RoomSharedFiles
            .FirstOrDefaultAsync(f => f.Id == fileId);
        
        if (file == null) return false;

        if (file.UploadedBy != userId) return false;

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        _context.RoomSharedFiles.Remove(file);
        var result = await _context.SaveChangesAsync();

        return result > 0;
    }

    private static string GetFileType(string contentType)
    {
        if (contentType.StartsWith("image/")) return "image";
        if (contentType.StartsWith("video/")) return "video";
        if (contentType.StartsWith("audio/")) return "audio";
        if (contentType.Contains("pdf")) return "pdf";
        if (contentType.Contains("document") || contentType.Contains("word")) return "document";
        if (contentType.Contains("sheet") || contentType.Contains("excel")) return "spreadsheet";
        if (contentType.Contains("presentation") || contentType.Contains("powerpoint")) return "presentation";
        if (contentType.Contains("zip") || contentType.Contains("rar") || contentType.Contains("archive")) return "archive";
        return "other";
    }
}

public class RoomSettingsService : IRoomSettingsService
{
    private readonly BookstoreDbContext _context;
    private readonly string _uploadFolder;

    public RoomSettingsService(BookstoreDbContext context)
    {
        _context = context;
        _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "backgrounds");
        Directory.CreateDirectory(_uploadFolder);
    }

    public async Task<RoomSetting?> GetSettingsAsync(long spaceId)
    {
        return await _context.RoomSettings
            .FirstOrDefaultAsync(s => s.SpaceId == spaceId);
    }

    public async Task<RoomSetting> UpdateBackgroundAsync(long spaceId, long userId, string backgroundType, string? backgroundValue, string? backgroundImagePath)
    {
        var setting = await _context.RoomSettings
            .FirstOrDefaultAsync(s => s.SpaceId == spaceId);

        if (setting == null)
        {
            setting = new RoomSetting
            {
                SpaceId = spaceId,
                BackgroundType = backgroundType,
                BackgroundValue = backgroundValue,
                BackgroundImagePath = backgroundImagePath,
                UpdatedBy = userId,
                UpdatedAt = DateTime.UtcNow
            };
            _context.RoomSettings.Add(setting);
        }
        else
        {
            setting.BackgroundType = backgroundType;
            setting.BackgroundValue = backgroundValue;
            setting.BackgroundImagePath = backgroundImagePath;
            setting.UpdatedBy = userId;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return setting;
    }

    public async Task<RoomSetting> UpdateAccentColorAsync(long spaceId, long userId, string accentColor)
    {
        var setting = await _context.RoomSettings
            .FirstOrDefaultAsync(s => s.SpaceId == spaceId);

        if (setting == null)
        {
            setting = new RoomSetting
            {
                SpaceId = spaceId,
                AccentColor = accentColor,
                UpdatedBy = userId,
                UpdatedAt = DateTime.UtcNow
            };
            _context.RoomSettings.Add(setting);
        }
        else
        {
            setting.AccentColor = accentColor;
            setting.UpdatedBy = userId;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return setting;
    }

    public async Task<string> UploadBackgroundImageAsync(long userId, byte[] imageData, string fileName)
    {
        var userFolder = Path.Combine(_uploadFolder, userId.ToString());
        Directory.CreateDirectory(userFolder);

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(userFolder, uniqueFileName);

        await File.WriteAllBytesAsync(filePath, imageData);

        return $"/uploads/backgrounds/{userId}/{uniqueFileName}";
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface IRoomMusicService
{
    Task<IEnumerable<RoomMusicTrack>> GetTracksAsync(long spaceId);
    Task<RoomMusicTrack> AddTrackAsync(long spaceId, long userId, string title, string? artist, string sourceType, string? filePath, string? externalUrl, int durationSeconds);
    Task<bool> DeleteTrackAsync(long trackId, long userId);
    Task<string> UploadMusicFileAsync(long userId, byte[] fileData, string fileName);
}

public interface IRoomFileService
{
    Task<IEnumerable<RoomSharedFile>> GetFilesAsync(long spaceId);
    Task<RoomSharedFile> UploadFileAsync(long spaceId, long userId, byte[] fileData, string fileName, string contentType);
    Task<(byte[] fileData, string fileName, string contentType)?> DownloadFileAsync(long fileId, long userId);
    Task<bool> DeleteFileAsync(long fileId, long userId);
}

public interface IRoomSettingsService
{
    Task<RoomSetting?> GetSettingsAsync(long spaceId);
    Task<RoomSetting> UpdateBackgroundAsync(long spaceId, long userId, string backgroundType, string? backgroundValue, string? backgroundImagePath);
    Task<RoomSetting> UpdateAccentColorAsync(long spaceId, long userId, string accentColor);
    Task<string> UploadBackgroundImageAsync(long userId, byte[] imageData, string fileName);
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website_Documents.API.DTOs;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/room")]
[Authorize]
public class RoomController : ControllerBase
{
    private readonly IRoomMusicService _musicService;
    private readonly IRoomFileService _fileService;
    private readonly IRoomSettingsService _settingsService;

    public RoomController(
        IRoomMusicService musicService,
        IRoomFileService fileService,
        IRoomSettingsService settingsService)
    {
        _musicService = musicService;
        _fileService = fileService;
        _settingsService = settingsService;
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    #region Music Endpoints

    [HttpGet("{spaceId}/music")]
    public async Task<IActionResult> GetMusicTracks(long spaceId)
    {
        var tracks = await _musicService.GetTracksAsync(spaceId);
        var result = new List<object>();
        foreach (var track in tracks)
        {
            result.Add(new
            {
                track.Id,
                track.SpaceId,
                track.Title,
                track.Artist,
                track.SourceType,
                track.FilePath,
                track.ExternalUrl,
                track.DurationSeconds,
                UploadedBy = track.UploadedBy,
                UploaderName = track.Uploader?.FullName,
                track.CreatedAt
            });
        }
        return Ok(ApiResponse<object>.SuccessResponse(result));
    }

    [HttpPost("{spaceId}/music/link")]
    public async Task<IActionResult> AddMusicFromLink(long spaceId, [FromBody] AddMusicLinkRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var track = await _musicService.AddTrackAsync(
            spaceId, userId.Value, request.Title, request.Artist, "link",
            null, request.Url, request.DurationSeconds);

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            track.Id,
            track.Title,
            track.Artist,
            track.SourceType,
            track.ExternalUrl,
            track.DurationSeconds
        }, "Track added successfully"));
    }

    [HttpPost("{spaceId}/music/upload")]
    public async Task<IActionResult> UploadMusic(long spaceId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var file = Request.Form.Files.FirstOrDefault();
        if (file == null)
            return BadRequest(ApiResponse<object>.ErrorResponse("No file uploaded"));

        var title = Request.Form["title"].ToString();
        var artist = Request.Form["artist"].ToString();
        var duration = int.TryParse(Request.Form["duration"].ToString(), out var d) ? d : 0;

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var filePath = await _musicService.UploadMusicFileAsync(userId.Value, memoryStream.ToArray(), file.FileName);

        var track = await _musicService.AddTrackAsync(
            spaceId, userId.Value, title, artist, "upload",
            filePath, null, duration);

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            track.Id,
            track.Title,
            track.Artist,
            track.SourceType,
            track.FilePath,
            track.DurationSeconds
        }, "Music uploaded successfully"));
    }

    [HttpDelete("music/{trackId}")]
    public async Task<IActionResult> DeleteMusicTrack(long trackId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _musicService.DeleteTrackAsync(trackId, userId.Value);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not delete track. You may not have permission or the track does not exist."));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Track deleted successfully"));
    }

    #endregion

    #region File Sharing Endpoints

    [HttpGet("{spaceId}/files")]
    public async Task<IActionResult> GetFiles(long spaceId)
    {
        var files = await _fileService.GetFilesAsync(spaceId);
        var result = new List<object>();
        foreach (var file in files)
        {
            result.Add(new
            {
                file.Id,
                file.SpaceId,
                file.FileName,
                file.OriginalName,
                file.FilePath,
                file.FileSize,
                file.ContentType,
                file.FileType,
                UploadedBy = file.UploadedBy,
                UploaderName = file.Uploader?.FullName,
                file.CreatedAt
            });
        }
        return Ok(ApiResponse<object>.SuccessResponse(result));
    }

    [HttpPost("{spaceId}/files/upload")]
    public async Task<IActionResult> UploadFile(long spaceId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var file = Request.Form.Files.FirstOrDefault();
        if (file == null)
            return BadRequest(ApiResponse<object>.ErrorResponse("No file uploaded"));

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var uploadedFile = await _fileService.UploadFileAsync(
            spaceId, userId.Value, memoryStream.ToArray(),
            file.FileName, file.ContentType);

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            uploadedFile.Id,
            uploadedFile.OriginalName,
            uploadedFile.FilePath,
            uploadedFile.FileSize,
            uploadedFile.FileType,
            uploadedFile.ContentType
        }, "File uploaded successfully"));
    }

    [HttpGet("files/{fileId}/download")]
    public async Task<IActionResult> DownloadFile(long fileId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var result = await _fileService.DownloadFileAsync(fileId, userId.Value);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("File not found"));

        var (fileData, fileName, contentType) = result.Value;
        return File(fileData, contentType, fileName);
    }

    [HttpDelete("files/{fileId}")]
    public async Task<IActionResult> DeleteFile(long fileId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _fileService.DeleteFileAsync(fileId, userId.Value);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not delete file. You may not have permission or the file does not exist."));

        return Ok(ApiResponse<object>.SuccessResponse(null, "File deleted successfully"));
    }

    #endregion

    #region Room Settings Endpoints

    [HttpGet("{spaceId}/settings")]
    public async Task<IActionResult> GetSettings(long spaceId)
    {
        var settings = await _settingsService.GetSettingsAsync(spaceId);
        if (settings == null)
        {
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                spaceId,
                backgroundType = "theme",
                backgroundValue = "aurora",
                accentColor = "#10b981"
            }));
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            settings.Id,
            settings.SpaceId,
            settings.BackgroundType,
            settings.BackgroundValue,
            settings.BackgroundImagePath,
            settings.AccentColor,
            settings.UpdatedAt
        }));
    }

    [HttpPut("{spaceId}/settings/background")]
    public async Task<IActionResult> UpdateBackground(long spaceId, [FromBody] UpdateBackgroundRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var settings = await _settingsService.UpdateBackgroundAsync(
            spaceId, userId.Value, request.BackgroundType,
            request.BackgroundValue, request.BackgroundImagePath);

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            settings.BackgroundType,
            settings.BackgroundValue,
            settings.BackgroundImagePath
        }, "Background updated successfully"));
    }

    [HttpPost("{spaceId}/settings/background/upload")]
    public async Task<IActionResult> UploadBackgroundImage(long spaceId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var file = Request.Form.Files.FirstOrDefault();
        if (file == null)
            return BadRequest(ApiResponse<object>.ErrorResponse("No file uploaded"));

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var imagePath = await _settingsService.UploadBackgroundImageAsync(
            userId.Value, memoryStream.ToArray(), file.FileName);

        var settings = await _settingsService.UpdateBackgroundAsync(
            spaceId, userId.Value, "custom", null, imagePath);

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            settings.BackgroundImagePath,
            imageUrl = imagePath
        }, "Background image uploaded successfully"));
    }

    [HttpPut("{spaceId}/settings/accent")]
    public async Task<IActionResult> UpdateAccentColor(long spaceId, [FromBody] UpdateAccentColorRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var settings = await _settingsService.UpdateAccentColorAsync(
            spaceId, userId.Value, request.AccentColor);

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            settings.AccentColor
        }, "Accent color updated successfully"));
    }

    #endregion
}

// Request DTOs
public class AddMusicLinkRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public string Url { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
}

public class UpdateBackgroundRequest
{
    public string BackgroundType { get; set; } = "theme";
    public string? BackgroundValue { get; set; }
    public string? BackgroundImagePath { get; set; }
}

public class UpdateAccentColorRequest
{
    public string AccentColor { get; set; } = string.Empty;
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly ILogger<UploadController> _logger;

    public UploadController(ILogger<UploadController> logger)
    {
        _logger = logger;
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Upload an image file
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });

            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "Không có file được tải lên" });

            // Validate file type (only images)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { success = false, message = "Định dạng file không được hỗ trợ. Chỉ chấp nhận: jpg, jpeg, png, gif, webp" });

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { success = false, message = "Kích thước file không được vượt quá 5MB" });

            // Create folder to save file
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "forum", userId.ToString());
            Directory.CreateDirectory(folder);

            // Create unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative URL
            var url = $"/uploads/forum/{userId}/{fileName}";

            _logger.LogInformation("Image uploaded successfully: {Url} by user {UserId}", url, userId);

            return Ok(new
            {
                success = true,
                url,
                message = "Upload ảnh thành công"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(500, new { success = false, message = "Lỗi khi upload ảnh" });
        }
    }
}

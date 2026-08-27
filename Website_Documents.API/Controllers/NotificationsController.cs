using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.API.DTOs;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var result = await _notificationService.GetUserNotificationsAsync(userId.Value, page, pageSize);
        return Ok(ApiResponse<List<object>>.SuccessResponse(result.ConvertAll(x => (object)x)));
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var result = await _notificationService.GetUnreadNotificationsAsync(userId.Value);
        return Ok(ApiResponse<List<object>>.SuccessResponse(result.ConvertAll(x => (object)x)));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var count = await _notificationService.GetUnreadCountAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(new { count }));
    }

    [HttpPost("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(long notificationId)
    {
        await _notificationService.MarkAsReadAsync(notificationId);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Notification marked as read"));
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        await _notificationService.MarkAllAsReadAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(null, "All notifications marked as read"));
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}

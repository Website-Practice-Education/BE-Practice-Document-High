using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Website_Documents.API.DTOs;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;

    public ProgressController(IProgressService progressService)
    {
        _progressService = progressService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var result = await _progressService.GetDashboardAsync(userId.Value);
        return Ok(ApiResponse<DashboardResponse>.SuccessResponse(result));
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetTodayProgress()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var result = await _progressService.GetTodayProgressAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(result));
    }

    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklyProgress()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var result = await _progressService.GetWeeklyProgressAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(result));
    }

    [HttpPost("lesson/{lessonId}")]
    public async Task<IActionResult> UpdateLessonProgress(int lessonId, [FromBody] UpdateProgressRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        await _progressService.UpdateLessonProgressAsync(userId.Value, lessonId, request.Status);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Lesson progress updated"));
    }

    [HttpPost("topic/{topicId}")]
    public async Task<IActionResult> UpdateTopicProgress(int topicId, [FromBody] UpdateProgressRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        await _progressService.UpdateTopicProgressAsync(userId.Value, topicId, request.Status);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Topic progress updated"));
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}

public class UpdateProgressRequest
{
    public string Status { get; set; } = "in_progress";
}

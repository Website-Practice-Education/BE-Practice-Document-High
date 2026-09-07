using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website_Documents.API.DTOs;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AchievementController : ControllerBase
{
    private readonly IAchievementService _achievementService;

    public AchievementController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    /// <summary>
    /// Get all achievements
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var achievements = await _achievementService.GetActiveAchievementsAsync();
        return Ok(ApiResponse<object>.SuccessResponse(achievements, "Achievements retrieved successfully"));
    }

    /// <summary>
    /// Get achievement by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var achievement = await _achievementService.GetAchievementByIdAsync(id);
        if (achievement == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Achievement not found"));

        return Ok(ApiResponse<object>.SuccessResponse(achievement, "Achievement retrieved successfully"));
    }

    /// <summary>
    /// Get current user's achievements
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyAchievements()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var achievements = await _achievementService.GetUserAchievementsAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(achievements, "User achievements retrieved successfully"));
    }

    /// <summary>
    /// Check and award achievements for current user
    /// </summary>
    [HttpPost("check")]
    public async Task<IActionResult> CheckAchievements()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var newAchievements = await _achievementService.CheckAndAwardAchievementsAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(newAchievements, "Achievements checked successfully"));
    }

    /// <summary>
    /// Award a specific achievement to current user (admin only)
    /// </summary>
    [HttpPost("award/{achievementId}")]
    public async Task<IActionResult> AwardAchievement(int achievementId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _achievementService.AwardAchievementAsync(userId.Value, achievementId);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Achievement already awarded or not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Achievement awarded successfully"));
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("user_id") ?? User.FindFirst("sub");
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
            return userId;
        return null;
    }
}

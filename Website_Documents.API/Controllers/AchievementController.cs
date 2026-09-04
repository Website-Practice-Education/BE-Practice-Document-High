using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
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

    [HttpGet]
    public async Task<IActionResult> GetAllAchievements()
    {
        var achievements = await _achievementService.GetAllAchievementsAsync();
        return Ok(ApiResponse<object>.SuccessResponse(achievements));
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyAchievements()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var achievements = await _achievementService.GetUserAchievementsAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(achievements));
    }

    [HttpPost("check")]
    public async Task<IActionResult> CheckAchievements()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var newAchievements = await _achievementService.CheckAndAwardAchievementsAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(newAchievements, "Achievements checked"));
    }

    [HttpPost("unlock/{code}")]
    public async Task<IActionResult> UnlockAchievement(string code)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var result = await _achievementService.UnlockAchievementAsync(userId.Value, code);
        if (result == null)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not unlock achievement"));

        return Ok(ApiResponse<object>.SuccessResponse(result, "Achievement unlocked!"));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var totalRewards = await _achievementService.CalculateTotalXPRewardsAsync(userId.Value);
        var myAchievements = await _achievementService.GetUserAchievementsAsync(userId.Value);
        var allAchievements = await _achievementService.GetAllAchievementsAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            TotalXPRewards = totalRewards,
            UnlockedCount = myAchievements.Count,
            TotalCount = allAchievements.Count,
            ProgressPercent = allAchievements.Count > 0 
                ? (myAchievements.Count * 100.0 / allAchievements.Count) 
                : 0
        }));
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}

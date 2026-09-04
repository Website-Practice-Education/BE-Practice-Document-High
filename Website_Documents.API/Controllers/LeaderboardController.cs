using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Website_Documents.API.DTOs;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLeaderboard([FromQuery] string type = "all", [FromQuery] int limit = 50)
    {
        var leaderboard = type.ToLower() switch
        {
            "weekly" => await _leaderboardService.GetWeeklyLeaderboardAsync(limit),
            "monthly" => await _leaderboardService.GetMonthlyLeaderboardAsync(limit),
            _ => await _leaderboardService.GetGlobalLeaderboardAsync(limit)
        };

        return Ok(ApiResponse<object>.SuccessResponse(leaderboard));
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserRank(long userId)
    {
        var rank = await _leaderboardService.GetUserRankAsync(userId);
        if (rank == null)
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

        return Ok(ApiResponse<object>.SuccessResponse(rank));
    }

    [HttpGet("my-rank")]
    public async Task<IActionResult> GetMyRank()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var rank = await _leaderboardService.GetUserRankAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(rank));
    }

    [HttpGet("top/{type}")]
    public async Task<IActionResult> GetTopUsers(string type, [FromQuery] int limit = 10)
    {
        var topUsers = await _leaderboardService.GetTopUsersAsync(type, limit);
        return Ok(ApiResponse<object>.SuccessResponse(topUsers));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var globalRank = await _leaderboardService.GetUserGlobalRankAsync(userId.Value);
        var weeklyLeaderboard = await _leaderboardService.GetWeeklyLeaderboardAsync(100);
        var myWeeklyRank = 0;
        for (int i = 0; i < weeklyLeaderboard.Count; i++)
        {
            if (weeklyLeaderboard[i] is System.Text.Json.JsonElement je && je.TryGetProperty("userId", out var uid) && uid.GetInt64() == userId)
            {
                myWeeklyRank = i + 1;
                break;
            }
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            GlobalRank = globalRank ?? 0,
            WeeklyRank = myWeeklyRank,
            TotalParticipants = weeklyLeaderboard.Count
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

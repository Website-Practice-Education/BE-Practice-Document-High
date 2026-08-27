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
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    // ===== Spaced Repetition System =====

    [HttpPost("card/{questionId}")]
    public async Task<IActionResult> CreateReviewCard(long questionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var card = await _reviewService.CreateReviewCardAsync(userId.Value, questionId);
        return Ok(ApiResponse<object>.SuccessResponse(card, "Review card created"));
    }

    [HttpGet("card/{questionId}")]
    public async Task<IActionResult> GetReviewCard(long questionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var card = await _reviewService.GetReviewCardAsync(userId.Value, questionId);
        return Ok(ApiResponse<object>.SuccessResponse(card));
    }

    [HttpPut("card/{questionId}")]
    public async Task<IActionResult> UpdateReview(long questionId, [FromBody] UpdateReviewRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var card = await _reviewService.UpdateReviewAsync(userId.Value, questionId, request.Rating);
        return Ok(ApiResponse<object>.SuccessResponse(card, "Review updated"));
    }

    [HttpGet("due")]
    public async Task<IActionResult> GetDueCards([FromQuery] int limit = 20)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var cards = await _reviewService.GetDueReviewCardsAsync(userId.Value, limit);
        return Ok(ApiResponse<object>.SuccessResponse(cards));
    }

    [HttpGet("due/count")]
    public async Task<IActionResult> GetDueCount()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var count = await _reviewService.GetDueReviewCountAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(new { dueCards = count }));
    }

    // ===== Review Session =====

    [HttpPost("session/start")]
    public async Task<IActionResult> StartReviewSession()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var session = await _reviewService.StartReviewSessionAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(session, "Review session started"));
    }

    [HttpGet("session/{sessionId}")]
    public async Task<IActionResult> GetReviewSession(long sessionId)
    {
        var session = await _reviewService.GetReviewSessionAsync(sessionId);
        if (session == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Session not found"));

        return Ok(ApiResponse<object>.SuccessResponse(session));
    }

    [HttpPost("session/{sessionId}/complete")]
    public async Task<IActionResult> CompleteReviewSession(long sessionId)
    {
        var session = await _reviewService.CompleteReviewSessionAsync(sessionId);
        return Ok(ApiResponse<object>.SuccessResponse(session, "Review session completed"));
    }

    // ===== Learning Analytics =====

    [HttpGet("analytics")]
    public async Task<IActionResult> GetReviewAnalytics()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var analytics = await _reviewService.GetReviewAnalyticsAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(analytics));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetReviewHistory(
        [FromQuery] string? fromDate = null,
        [FromQuery] string? toDate = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        System.DateTime? from = null;
        System.DateTime? to = null;

        if (!string.IsNullOrEmpty(fromDate) && System.DateTime.TryParse(fromDate, out var parsedFrom))
            from = parsedFrom;

        if (!string.IsNullOrEmpty(toDate) && System.DateTime.TryParse(toDate, out var parsedTo))
            to = parsedTo;

        var history = await _reviewService.GetReviewHistoryAsync(userId.Value, from, to);
        return Ok(ApiResponse<object>.SuccessResponse(history));
    }
}

public class UpdateReviewRequest
{
    public ReviewRating Rating { get; set; }
}

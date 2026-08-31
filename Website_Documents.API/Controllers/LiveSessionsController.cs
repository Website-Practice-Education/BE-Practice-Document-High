using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website_Documents.API.DTOs;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LiveSessionsController : ControllerBase
{
    private readonly ILiveSessionService _liveSessionService;
    private readonly IWhiteboardService _whiteboardService;

    public LiveSessionsController(
        ILiveSessionService liveSessionService,
        IWhiteboardService whiteboardService)
    {
        _liveSessionService = liveSessionService;
        _whiteboardService = whiteboardService;
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var result = await _liveSessionService.CreateSessionAsync(
            userId.Value,
            request.Title,
            request.Description,
            request.SessionType,
            request.SubjectId,
            request.TopicId,
            request.DifficultyLevel,
            request.QuestionCount,
            request.TimeLimitMinutes,
            request.SpaceId);

        return Ok(ApiResponse<object>.SuccessResponse(result, "Session created successfully"));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSession(long id)
    {
        var session = await _liveSessionService.GetSessionByIdAsync(id);
        if (session == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Session not found"));

        return Ok(ApiResponse<object>.SuccessResponse(session));
    }

    [HttpGet("by-code/{inviteCode}")]
    public async Task<IActionResult> GetSessionByCode(string inviteCode)
    {
        var session = await _liveSessionService.GetSessionByInviteCodeAsync(inviteCode);
        if (session == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Session not found"));

        return Ok(ApiResponse<object>.SuccessResponse(session));
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveSessions()
    {
        var sessions = await _liveSessionService.GetActiveSessionsAsync();
        return Ok(ApiResponse<object>.SuccessResponse(sessions));
    }

    [HttpGet("my-sessions")]
    public async Task<IActionResult> GetMySessions()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var sessions = await _liveSessionService.GetMySessionsAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(sessions));
    }

    [HttpGet("space/{spaceId}")]
    public async Task<IActionResult> GetSessionsBySpace(long spaceId)
    {
        var sessions = await _liveSessionService.GetSessionsBySpaceAsync(spaceId);
        return Ok(ApiResponse<object>.SuccessResponse(sessions));
    }

    [HttpPost("{id}/join")]
    public async Task<IActionResult> JoinSession(long id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _liveSessionService.JoinSessionAsync(id, userId.Value);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not join session"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Joined session successfully"));
    }

    [HttpPost("{id}/leave")]
    public async Task<IActionResult> LeaveSession(long id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _liveSessionService.LeaveSessionAsync(id, userId.Value);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not leave session (you may be the host)"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Left session successfully"));
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartSession(long id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _liveSessionService.StartSessionAsync(id, userId.Value);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not start session"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Session started"));
    }

    [HttpPost("{id}/end")]
    public async Task<IActionResult> EndSession(long id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _liveSessionService.EndSessionAsync(id, userId.Value);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not end session"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Session ended"));
    }

    [HttpPost("{id}/ready")]
    public async Task<IActionResult> SetReady(long id, [FromBody] SetReadyRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _liveSessionService.SetReadyAsync(id, userId.Value, request.IsReady);
        return Ok(ApiResponse<object>.SuccessResponse(null, success ? "Ready status updated" : "Failed to update ready status"));
    }

    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetMembers(long id)
    {
        var members = await _liveSessionService.GetSessionMembersAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(members));
    }

    [HttpGet("{id}/leaderboard")]
    public async Task<IActionResult> GetLeaderboard(long id)
    {
        var leaderboard = await _liveSessionService.GetLeaderboardAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(leaderboard));
    }

    [HttpPost("{id}/answer")]
    public async Task<IActionResult> SubmitAnswer(long id, [FromBody] SubmitLiveSessionAnswerRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _liveSessionService.SubmitAnswerAsync(
            id, userId.Value, request.QuestionId, request.OptionId, request.Letter, request.TimeSpentSeconds);

        return Ok(ApiResponse<object>.SuccessResponse(null, success ? "Answer submitted" : "Failed to submit answer"));
    }

    [HttpGet("{id}/current-question")]
    public async Task<IActionResult> GetCurrentQuestion(long id)
    {
        var question = await _liveSessionService.GetCurrentQuestionAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(question ?? new { }));
    }

    [HttpPost("{id}/current-question")]
    public async Task<IActionResult> SetCurrentQuestion(long id, [FromBody] SetCurrentQuestionRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _liveSessionService.SetCurrentQuestionAsync(id, userId.Value, request.QuestionId);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not set current question"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Current question updated"));
    }

    [HttpGet("{id}/activities")]
    public async Task<IActionResult> GetActivities(long id, [FromQuery] int limit = 50)
    {
        var activities = await _liveSessionService.GetSessionActivitiesAsync(id, limit);
        return Ok(ApiResponse<object>.SuccessResponse(activities));
    }

    [HttpGet("{id}/chat")]
    public async Task<IActionResult> GetChat(long id, [FromQuery] int limit = 100)
    {
        var messages = await _liveSessionService.GetChatMessagesAsync(id, limit);
        return Ok(ApiResponse<object>.SuccessResponse(messages));
    }

    [HttpPost("{id}/chat")]
    public async Task<IActionResult> SendChat(long id, [FromBody] SendChatRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _liveSessionService.SendChatMessageAsync(id, userId.Value, request.Content, request.MessageType);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to send message"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Message sent"));
    }

    [HttpGet("{id}/whiteboard")]
    public async Task<IActionResult> GetWhiteboard(long id)
    {
        var items = await _whiteboardService.GetWhiteboardItemsAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(items));
    }

    [HttpPost("{id}/whiteboard/text")]
    public async Task<IActionResult> AddWhiteboardText(long id, [FromBody] AddWhiteboardTextRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var result = await _whiteboardService.AddTextElementAsync(id, userId.Value, request.Text, request.X, request.Y, request.Color, request.FontSize);
        return Ok(ApiResponse<object>.SuccessResponse(result, "Text added"));
    }

    [HttpPost("{id}/whiteboard/drawing")]
    public async Task<IActionResult> AddWhiteboardDrawing(long id, [FromBody] AddWhiteboardDrawingRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var result = await _whiteboardService.AddDrawingElementAsync(id, userId.Value, request.DrawingData, request.X, request.Y, request.Color, request.Width, request.Height);
        return Ok(ApiResponse<object>.SuccessResponse(result, "Drawing added"));
    }

    [HttpPut("whiteboard/{elementId}")]
    public async Task<IActionResult> UpdateWhiteboardElement(long elementId, [FromBody] UpdateWhiteboardElementRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _whiteboardService.UpdateElementAsync(elementId, userId.Value, request.Content, request.X, request.Y);
        return Ok(ApiResponse<object>.SuccessResponse(null, success ? "Element updated" : "Failed to update element"));
    }

    [HttpDelete("whiteboard/{elementId}")]
    public async Task<IActionResult> DeleteWhiteboardElement(long elementId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _whiteboardService.DeleteElementAsync(elementId, userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(null, success ? "Element deleted" : "Failed to delete element"));
    }

    [HttpDelete("{id}/whiteboard/clear")]
    public async Task<IActionResult> ClearWhiteboard(long id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _whiteboardService.ClearWhiteboardAsync(id, userId.Value);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Only host can clear whiteboard"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Whiteboard cleared"));
    }
}

// Request DTOs
public class CreateSessionRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SessionType { get; set; } = "practice";
    public int? SubjectId { get; set; }
    public int? TopicId { get; set; }
    public int DifficultyLevel { get; set; } = 1;
    public int QuestionCount { get; set; } = 10;
    public int TimeLimitMinutes { get; set; } = 30;
    public long? SpaceId { get; set; }
}

public class SetReadyRequest
{
    public bool IsReady { get; set; }
}

public class SubmitLiveSessionAnswerRequest
{
    public long QuestionId { get; set; }
    public long? OptionId { get; set; }
    public char? Letter { get; set; }
    public int TimeSpentSeconds { get; set; }
}

public class SetCurrentQuestionRequest
{
    public long QuestionId { get; set; }
}

public class SendChatRequest
{
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "text";
}

public class AddWhiteboardTextRequest
{
    public string Text { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public string? Color { get; set; }
    public int? FontSize { get; set; }
}

public class AddWhiteboardDrawingRequest
{
    public string DrawingData { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public string Color { get; set; } = "#000000";
    public int? Width { get; set; }
    public int? Height { get; set; }
}

public class UpdateWhiteboardElementRequest
{
    public string Content { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
}

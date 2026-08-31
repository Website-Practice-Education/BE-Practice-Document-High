using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website_Documents.API.DTOs;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/call")]
[Authorize]
public class CallController : ControllerBase
{
    private readonly ICallService _callService;
    private readonly IStudySpaceService _studySpaceService;

    public CallController(ICallService callService, IStudySpaceService studySpaceService)
    {
        _callService = callService;
        _studySpaceService = studySpaceService;
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    /// <summary>
    /// Start a new call session in a space
    /// </summary>
    [HttpPost("start/{spaceId}")]
    public async Task<IActionResult> StartCall(long spaceId, [FromBody] StartCallRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        // Check if user is a member of the space
        var isMember = await _studySpaceService.IsMemberAsync(spaceId, userId.Value);
        if (!isMember)
            return BadRequest(ApiResponse<object>.ErrorResponse("You must be a member of the space to start a call"));

        try
        {
            var session = await _callService.CreateCallSessionAsync(spaceId, userId.Value, request.CallType);
            return Ok(ApiResponse<object>.SuccessResponse(session, "Call started successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get active call for a space
    /// </summary>
    [HttpGet("active/{spaceId}")]
    public async Task<IActionResult> GetActiveCall(long spaceId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var session = await _callService.GetActiveCallForSpaceAsync(spaceId);
        if (session == null)
            return NotFound(ApiResponse<object>.ErrorResponse("No active call in this space"));

        return Ok(ApiResponse<object>.SuccessResponse(session));
    }

    /// <summary>
    /// Get call session details
    /// </summary>
    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetCallSession(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var session = await _callService.GetCallSessionAsync(sessionId);
        if (session == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Call session not found"));

        return Ok(ApiResponse<object>.SuccessResponse(session));
    }

    /// <summary>
    /// End a call session
    /// </summary>
    [HttpPost("{sessionId}/end")]
    public async Task<IActionResult> EndCall(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _callService.EndCallSessionAsync(sessionId, userId.Value);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not end call (only the initiator can end)"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Call ended successfully"));
    }

    /// <summary>
    /// Join an existing call
    /// </summary>
    [HttpPost("{sessionId}/join")]
    public async Task<IActionResult> JoinCall(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        try
        {
            var participant = await _callService.JoinCallAsync(sessionId, userId.Value);
            if (participant == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Call session not found or has ended"));

            return Ok(ApiResponse<object>.SuccessResponse(participant, "Joined call successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Leave a call
    /// </summary>
    [HttpPost("{sessionId}/leave")]
    public async Task<IActionResult> LeaveCall(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _callService.LeaveCallAsync(sessionId, userId.Value);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not leave call"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Left call successfully"));
    }

    /// <summary>
    /// Get participants in a call
    /// </summary>
    [HttpGet("{sessionId}/participants")]
    public async Task<IActionResult> GetParticipants(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var participants = await _callService.GetCallParticipantsAsync(sessionId);
        return Ok(ApiResponse<object>.SuccessResponse(participants));
    }

    /// <summary>
    /// Toggle mute for current user
    /// </summary>
    [HttpPost("{sessionId}/mute")]
    public async Task<IActionResult> ToggleMute(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _callService.ToggleMuteAsync(sessionId, userId.Value);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not toggle mute"));

        return Ok(ApiResponse<object>.SuccessResponse(new { isMuted = true }, "Muted successfully"));
    }

    /// <summary>
    /// Toggle video for current user
    /// </summary>
    [HttpPost("{sessionId}/video")]
    public async Task<IActionResult> ToggleVideo(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _callService.ToggleVideoAsync(sessionId, userId.Value);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not toggle video"));

        return Ok(ApiResponse<object>.SuccessResponse(new { isVideoOff = true }, "Video disabled successfully"));
    }

    /// <summary>
    /// Toggle screen share for current user
    /// </summary>
    [HttpPost("{sessionId}/screen-share")]
    public async Task<IActionResult> ToggleScreenShare(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _callService.ToggleScreenShareAsync(sessionId, userId.Value);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not toggle screen share"));

        return Ok(ApiResponse<object>.SuccessResponse(new { isScreenSharing = true }, "Screen sharing enabled successfully"));
    }

    /// <summary>
    /// Update connection status
    /// </summary>
    [HttpPost("{sessionId}/connection-status")]
    public async Task<IActionResult> UpdateConnectionStatus(long sessionId, [FromBody] UpdateConnectionStatusRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _callService.UpdateConnectionStatusAsync(sessionId, userId.Value, request.Status);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not update connection status"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Connection status updated"));
    }

    /// <summary>
    /// Check if user is currently in a call for a space
    /// </summary>
    [HttpGet("in-call/{spaceId}")]
    public async Task<IActionResult> IsInCall(long spaceId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var isInCall = await _callService.IsUserInCallAsync(spaceId, userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(new { isInCall }));
    }
}

public class StartCallRequest
{
    public string CallType { get; set; } = "audio"; // "audio" or "video"
}

public class UpdateConnectionStatusRequest
{
    public string Status { get; set; } = "connected"; // "connected", "disconnected", "reconnecting"
}

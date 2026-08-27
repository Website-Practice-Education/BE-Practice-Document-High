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
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IStudySpaceService _studySpaceService;

    public ChatController(IChatService chatService, IStudySpaceService studySpaceService)
    {
        _chatService = chatService;
        _studySpaceService = studySpaceService;
    }

    [HttpGet("{spaceId}/messages")]
    public async Task<IActionResult> GetMessages(long spaceId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var isMember = await _studySpaceService.IsMemberAsync(spaceId, userId.Value);
        if (!isMember)
            return Forbid();

        var messages = await _chatService.GetMessagesAsync(spaceId, page, pageSize);
        return Ok(ApiResponse<object>.SuccessResponse(messages.Select(m => new
        {
            id = m.Id,
            userId = m.UserId,
            userName = m.User?.FullName,
            userAvatar = m.User?.AvatarUrl,
            content = m.Content,
            messageType = m.MessageType,
            createdAt = m.CreatedAt
        })));
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}

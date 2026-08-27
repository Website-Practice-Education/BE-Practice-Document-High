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
public class StudySpacesController : ControllerBase
{
    private readonly IStudySpaceService _studySpaceService;

    public StudySpacesController(IStudySpaceService studySpaceService)
    {
        _studySpaceService = studySpaceService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSpace([FromBody] CreateSpaceRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var space = await _studySpaceService.CreateSpaceAsync(
            userId.Value, 
            request.Name, 
            request.Description, 
            request.SpaceType);

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            id = space.Id,
            name = space.Name,
            description = space.Description,
            spaceType = space.SpaceType,
            inviteCode = space.InviteCode,
            maxMembers = space.MaxMembers,
            createdAt = space.CreatedAt
        }, "Study space created successfully"));
    }

    [HttpGet]
    public async Task<IActionResult> GetMySpaces()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var spaces = await _studySpaceService.GetUserSpacesAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(spaces.Select(s => new
        {
            id = s.Id,
            name = s.Name,
            description = s.Description,
            spaceType = s.SpaceType,
            memberCount = s.Members?.Count ?? 0,
            createdAt = s.CreatedAt,
            creatorName = s.Creator?.FullName
        })));
    }

    [HttpGet("public")]
    public async Task<IActionResult> GetPublicSpaces([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var spaces = await _studySpaceService.GetPublicSpacesAsync(page, pageSize);
        return Ok(ApiResponse<object>.SuccessResponse(spaces.Select(s => new
        {
            id = s.Id,
            name = s.Name,
            description = s.Description,
            spaceType = s.SpaceType,
            memberCount = s.Members?.Count ?? 0,
            createdAt = s.CreatedAt,
            creatorName = s.Creator?.FullName
        })));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpace(long id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var space = await _studySpaceService.GetSpaceByIdAsync(id);
        if (space == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Space not found"));

        var members = await _studySpaceService.GetSpaceMembersAsync(id);
        
        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            id = space.Id,
            name = space.Name,
            description = space.Description,
            spaceType = space.SpaceType,
            inviteCode = space.InviteCode,
            maxMembers = space.MaxMembers,
            memberCount = members.Count,
            members = members.Select(m => new
            {
                id = m.UserId,
                name = m.User?.FullName,
                avatar = m.User?.AvatarUrl,
                role = m.Role,
                joinedAt = m.JoinedAt
            }),
            createdAt = space.CreatedAt,
            creatorName = space.Creator?.FullName,
            isMember = members.Exists(m => m.UserId == userId.Value)
        }));
    }

    [HttpPost("join/{id}")]
    public async Task<IActionResult> JoinSpace(long id, [FromBody] JoinSpaceRequest? request = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _studySpaceService.JoinSpaceAsync(id, userId.Value, request?.InviteCode);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not join space"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Joined space successfully"));
    }

    [HttpPost("join-by-code")]
    public async Task<IActionResult> JoinByCode([FromBody] JoinByCodeRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var space = await _studySpaceService.GetSpaceByInviteCodeAsync(request.InviteCode);
        if (space == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Invalid invite code"));

        var success = await _studySpaceService.JoinSpaceAsync(space.Id, userId.Value, request.InviteCode);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not join space"));

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            spaceId = space.Id,
            spaceName = space.Name
        }, "Joined space successfully"));
    }

    [HttpPost("leave/{id}")]
    public async Task<IActionResult> LeaveSpace(long id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _studySpaceService.LeaveSpaceAsync(id, userId.Value);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not leave space (owner cannot leave)"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Left space successfully"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSpace(long id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _studySpaceService.DeleteSpaceAsync(id, userId.Value);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not delete space"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Space deleted successfully"));
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}

public class CreateSpaceRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SpaceType { get; set; } = "public";
}

public class JoinSpaceRequest
{
    public string? InviteCode { get; set; }
}

public class JoinByCodeRequest
{
    public string InviteCode { get; set; } = string.Empty;
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website_Documents.API.DTOs;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendsController : ControllerBase
{
    private readonly IFriendshipService _friendshipService;

    public FriendsController(IFriendshipService friendshipService)
    {
        _friendshipService = friendshipService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var friends = await _friendshipService.GetFriendsAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(friends.Select(f => new
        {
            id = f.Id,
            friendId = f.UserId == userId.Value ? f.FriendId : f.UserId,
            friendName = f.UserId == userId.Value ? f.Friend?.FullName : f.User?.FullName,
            friendAvatar = f.UserId == userId.Value ? f.Friend?.AvatarUrl : f.User?.AvatarUrl,
            status = f.Status,
            createdAt = f.CreatedAt
        })));
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var requests = await _friendshipService.GetPendingRequestsAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(requests.Select(r => new
        {
            id = r.Id,
            userId = r.UserId,
            userName = r.User?.FullName,
            userAvatar = r.User?.AvatarUrl,
            createdAt = r.CreatedAt
        })));
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string q)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return BadRequest(ApiResponse<object>.ErrorResponse("Search term must be at least 2 characters"));

        var users = await _friendshipService.SearchUsersAsync(userId.Value, q);
        return Ok(ApiResponse<object>.SuccessResponse(users.Select(u => new
        {
            id = u.FriendId,
            name = u.Friend?.FullName,
            avatar = u.Friend?.AvatarUrl,
            email = u.Friend?.Email
        })));
    }

    [HttpPost("request/{friendId}")]
    public async Task<IActionResult> SendFriendRequest(long friendId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        try
        {
            var request = await _friendshipService.SendFriendRequestAsync(userId.Value, friendId);
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                id = request.Id,
                friendId = request.FriendId
            }, "Friend request sent"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("accept/{requestId}")]
    public async Task<IActionResult> AcceptRequest(long requestId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var result = await _friendshipService.AcceptFriendRequestAsync(requestId, userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Friend request accepted"));
    }

    [HttpPost("decline/{requestId}")]
    public async Task<IActionResult> DeclineRequest(long requestId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        await _friendshipService.DeclineFriendRequestAsync(requestId, userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Friend request declined"));
    }

    [HttpDelete("{friendId}")]
    public async Task<IActionResult> RemoveFriend(long friendId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var success = await _friendshipService.RemoveFriendAsync(userId.Value, friendId);
        if (!success)
            return BadRequest(ApiResponse<object>.ErrorResponse("Could not remove friend"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Friend removed"));
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}

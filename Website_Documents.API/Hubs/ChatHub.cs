using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public async Task JoinSpace(string spaceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"space_{spaceId}");
    }

    public async Task LeaveSpace(string spaceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"space_{spaceId}");
    }

    public async Task SendMessage(long spaceId, string content)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            await Clients.Caller.SendAsync("Error", new { message = "Unauthenticated user." });
            return;
        }

        try
        {
            var message = await _chatService.SendMessageAsync(spaceId, userId.Value, content);

            await Clients.Group($"space_{spaceId}").SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                spaceId = message.SpaceId,
                userId = message.UserId,
                userName = message.User?.FullName ?? "Unknown",
                userAvatar = message.User?.AvatarUrl,
                content = message.Content,
                messageType = message.MessageType,
                createdAt = message.CreatedAt
            });
        }
        catch (ArgumentException ex)
        {
            await Clients.Caller.SendAsync("Error", new { code = "INVALID_CONTENT", message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            await Clients.Caller.SendAsync("Error", new { code = "NOT_MEMBER", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            await Clients.Caller.SendAsync("Error", new { code = "SPACE_NOT_FOUND", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending message in space {SpaceId} by user {UserId}", spaceId, userId);
            // Log unexpected exceptions (the framework also logs them) and notify the caller gracefully
            await Clients.Caller.SendAsync("Error", new { code = "SEND_FAILED", message = "Failed to send message. Please try again." });
        }
    }

    public async Task SendTypingIndicator(long spaceId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        await Clients.OthersInGroup($"space_{spaceId}").SendAsync("UserTyping", new
        {
            userId = userId.Value,
            spaceId
        });
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} connected to ChatHub with connectionId {ConnectionId}", 
            userId?.ToString() ?? "anonymous", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        if (exception != null)
        {
            _logger.LogWarning(exception, "User {UserId} disconnected from ChatHub with error", userId?.ToString() ?? "anonymous");
        }
        else
        {
            _logger.LogInformation("User {UserId} disconnected from ChatHub", userId?.ToString() ?? "anonymous");
        }
        await base.OnDisconnectedAsync(exception);
    }
}

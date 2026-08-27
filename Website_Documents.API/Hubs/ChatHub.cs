using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
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
        if (userId == null) return;

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
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}

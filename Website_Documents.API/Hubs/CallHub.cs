using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Hubs;

[Authorize]
public class CallHub : Hub
{
    private readonly ICallService _callService;
    
    // Track user connections per call session
    private static readonly ConcurrentDictionary<string, string> ConnectionToRoom = new();
    private static readonly ConcurrentDictionary<string, long> ConnectionToUser = new();

    public CallHub(ICallService callService)
    {
        _callService = callService;
    }

    /// <summary>
    /// Join a call room
    /// </summary>
    public async Task JoinCall(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        try
        {
            var participant = await _callService.JoinCallAsync(sessionId, userId.Value);
            if (participant == null) return;

            var roomName = $"call_{sessionId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            
            // Track connection
            ConnectionToRoom[Context.ConnectionId] = roomName;
            ConnectionToUser[Context.ConnectionId] = userId.Value;

            // Notify others that user joined
            await Clients.OthersInGroup(roomName).SendAsync("UserJoined", new
            {
                userId = participant.UserId,
                userName = participant.UserName,
                userAvatar = participant.UserAvatar,
                peerId = participant.PeerId,
                isMuted = participant.IsMuted,
                isVideoOff = participant.IsVideoOff
            });

            // Send current participants to the joining user
            var participants = await _callService.GetCallParticipantsAsync(sessionId);
            await Clients.Caller.SendAsync("ParticipantsList", participants);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("CallError", ex.Message);
        }
    }

    /// <summary>
    /// Leave a call room
    /// </summary>
    public async Task LeaveCall(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        try
        {
            await _callService.LeaveCallAsync(sessionId, userId.Value);
            
            var roomName = $"call_{sessionId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);

            // Clean up tracking
            ConnectionToRoom.TryRemove(Context.ConnectionId, out _);
            ConnectionToUser.TryRemove(Context.ConnectionId, out _);

            // Notify others that user left
            await Clients.OthersInGroup(roomName).SendAsync("UserLeft", new
            {
                userId = userId.Value
            });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("CallError", ex.Message);
        }
    }

    /// <summary>
    /// Send WebRTC offer to a specific user
    /// </summary>
    public async Task SendOffer(long targetUserId, object offer)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        await Clients.Group($"user_{targetUserId}").SendAsync("ReceiveOffer", new
        {
            fromUserId = userId.Value,
            offer
        });
    }

    /// <summary>
    /// Send WebRTC answer to a specific user
    /// </summary>
    public async Task SendAnswer(long targetUserId, object answer)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        await Clients.Group($"user_{targetUserId}").SendAsync("ReceiveAnswer", new
        {
            fromUserId = userId.Value,
            answer
        });
    }

    /// <summary>
    /// Send ICE candidate to a specific user
    /// </summary>
    public async Task SendIceCandidate(long targetUserId, object candidate)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        await Clients.Group($"user_{targetUserId}").SendAsync("ReceiveIceCandidate", new
        {
            fromUserId = userId.Value,
            candidate
        });
    }

    /// <summary>
    /// Toggle mute
    /// </summary>
    public async Task ToggleMute(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        try
        {
            var success = await _callService.ToggleMuteAsync(sessionId, userId.Value);
            if (success)
            {
                var roomName = $"call_{sessionId}";
                await Clients.Group(roomName).SendAsync("ParticipantMuted", new
                {
                    userId = userId.Value,
                    isMuted = true
                });
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("CallError", ex.Message);
        }
    }

    /// <summary>
    /// Toggle video
    /// </summary>
    public async Task ToggleVideo(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        try
        {
            var success = await _callService.ToggleVideoAsync(sessionId, userId.Value);
            if (success)
            {
                var roomName = $"call_{sessionId}";
                await Clients.Group(roomName).SendAsync("ParticipantVideoChanged", new
                {
                    userId = userId.Value,
                    isVideoOff = true
                });
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("CallError", ex.Message);
        }
    }

    /// <summary>
    /// Toggle screen sharing
    /// </summary>
    public async Task ToggleScreenShare(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        try
        {
            var success = await _callService.ToggleScreenShareAsync(sessionId, userId.Value);
            if (success)
            {
                var roomName = $"call_{sessionId}";
                await Clients.Group(roomName).SendAsync("ParticipantScreenShareChanged", new
                {
                    userId = userId.Value,
                    isScreenSharing = true
                });
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("CallError", ex.Message);
        }
    }

    /// <summary>
    /// Update connection status (for reconnection handling)
    /// </summary>
    public async Task UpdateConnectionStatus(long sessionId, string status)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        try
        {
            await _callService.UpdateConnectionStatusAsync(sessionId, userId.Value, status);
            
            var roomName = $"call_{sessionId}";
            await Clients.OthersInGroup(roomName).SendAsync("ParticipantConnectionStatusChanged", new
            {
                userId = userId.Value,
                status
            });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("CallError", ex.Message);
        }
    }

    /// <summary>
    /// Subscribe to user-specific notifications (for WebRTC signaling)
    /// </summary>
    public async Task SubscribeToUser(long userId)
    {
        if (userId == GetCurrentUserId())
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
    }

    /// <summary>
    /// Unsubscribe from user notifications
    /// </summary>
    public async Task UnsubscribeFromUser(long userId)
    {
        if (userId == GetCurrentUserId())
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
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
        
        // Auto-subscribe user to their own notification group
        var userId = GetCurrentUserId();
        if (userId.HasValue)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Handle disconnect - update connection status for any active calls
        if (ConnectionToRoom.TryRemove(Context.ConnectionId, out var roomName))
        {
            if (ConnectionToUser.TryRemove(Context.ConnectionId, out var userId))
            {
                // Update connection status to disconnected
                if (roomName.StartsWith("call_") && long.TryParse(roomName.Replace("call_", ""), out var sessionId))
                {
                    try
                    {
                        await _callService.UpdateConnectionStatusAsync(sessionId, userId, "disconnected");
                        
                        await Clients.OthersInGroup(roomName).SendAsync("ParticipantConnectionStatusChanged", new
                        {
                            userId = userId,
                            status = "disconnected"
                        });
                    }
                    catch
                    {
                        // Ignore errors during disconnect
                    }
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Website_Documents.API.Hubs;

[Authorize]
public class LiveSessionHub : Hub
{
    private readonly ILogger<LiveSessionHub> _logger;

    public LiveSessionHub(ILogger<LiveSessionHub> logger)
    {
        _logger = logger;
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    public async Task JoinSession(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
        await Clients.Group($"session_{sessionId}").SendAsync("UserJoined", new
        {
            sessionId,
            userId,
            connectionId = Context.ConnectionId,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task LeaveSession(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");
        await Clients.Group($"session_{sessionId}").SendAsync("UserLeft", new
        {
            sessionId,
            userId,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task SendChatMessage(long sessionId, string message)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        await Clients.Group($"session_{sessionId}").SendAsync("NewChatMessage", new
        {
            sessionId,
            userId,
            message,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task UpdateReadyStatus(long sessionId, bool isReady)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        await Clients.Group($"session_{sessionId}").SendAsync("ReadyStatusChanged", new
        {
            sessionId,
            userId,
            isReady,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task ShowQuestion(long sessionId, long questionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        await Clients.Group($"session_{sessionId}").SendAsync("QuestionShown", new
        {
            sessionId,
            questionId,
            sharedBy = userId,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task SubmitAnswer(long sessionId, long questionId, char? answer)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        await Clients.Group($"session_{sessionId}").SendAsync("AnswerSubmitted", new
        {
            sessionId,
            userId,
            questionId,
            answer,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task ShowAnswerResult(long sessionId, long questionId, char correctAnswer)
    {
        await Clients.Group($"session_{sessionId}").SendAsync("AnswerRevealed", new
        {
            sessionId,
            questionId,
            correctAnswer,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task UpdateLeaderboard(long sessionId)
    {
        await Clients.Group($"session_{sessionId}").SendAsync("LeaderboardUpdated", new
        {
            sessionId,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task WhiteboardDraw(long sessionId, string drawingData)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        await Clients.OthersInGroup($"session_{sessionId}").SendAsync("WhiteboardDraw", new
        {
            sessionId,
            userId,
            drawingData,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task WhiteboardClear(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        await Clients.Group($"session_{sessionId}").SendAsync("WhiteboardCleared", new
        {
            sessionId,
            clearedBy = userId,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task SessionStarted(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        await Clients.Group($"session_{sessionId}").SendAsync("SessionStarted", new
        {
            sessionId,
            startedBy = userId,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task SessionEnded(long sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return;

        await Clients.Group($"session_{sessionId}").SendAsync("SessionEnded", new
        {
            sessionId,
            endedBy = userId,
            timestamp = DateTime.UtcNow
        });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        if (exception != null)
        {
            _logger.LogWarning(exception, "User {UserId} disconnected from LiveSessionHub with error", userId?.ToString() ?? "anonymous");
        }
        else
        {
            _logger.LogInformation("User {UserId} disconnected from LiveSessionHub", userId?.ToString() ?? "anonymous");
        }
        await base.OnDisconnectedAsync(exception);
    }
}

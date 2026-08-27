using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Website_Documents.API.Hubs;

[Authorize]
public class LiveSessionHub : Hub
{
    public async Task JoinSession(long sessionId, long userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
        await Clients.Group($"session_{sessionId}").SendAsync("UserJoined", new
        {
            sessionId,
            userId,
            connectionId = Context.ConnectionId,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task LeaveSession(long sessionId, long userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");
        await Clients.Group($"session_{sessionId}").SendAsync("UserLeft", new
        {
            sessionId,
            userId,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task SendChatMessage(long sessionId, long userId, string message)
    {
        await Clients.Group($"session_{sessionId}").SendAsync("NewChatMessage", new
        {
            sessionId,
            userId,
            message,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task UpdateReadyStatus(long sessionId, long userId, bool isReady)
    {
        await Clients.Group($"session_{sessionId}").SendAsync("ReadyStatusChanged", new
        {
            sessionId,
            userId,
            isReady,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task ShowQuestion(long sessionId, long questionId, long userId)
    {
        await Clients.Group($"session_{sessionId}").SendAsync("QuestionShown", new
        {
            sessionId,
            questionId,
            sharedBy = userId,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task SubmitAnswer(long sessionId, long userId, long questionId, char? answer)
    {
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

    public async Task WhiteboardDraw(long sessionId, long userId, string drawingData)
    {
        await Clients.OthersInGroup($"session_{sessionId}").SendAsync("WhiteboardDraw", new
        {
            sessionId,
            userId,
            drawingData,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task WhiteboardClear(long sessionId, long userId)
    {
        await Clients.Group($"session_{sessionId}").SendAsync("WhiteboardCleared", new
        {
            sessionId,
            clearedBy = userId,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task SessionStarted(long sessionId, long userId)
    {
        await Clients.Group($"session_{sessionId}").SendAsync("SessionStarted", new
        {
            sessionId,
            startedBy = userId,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task SessionEnded(long sessionId, long userId)
    {
        await Clients.Group($"session_{sessionId}").SendAsync("SessionEnded", new
        {
            sessionId,
            endedBy = userId,
            timestamp = DateTime.UtcNow
        });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}

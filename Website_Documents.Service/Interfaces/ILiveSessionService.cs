using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Website_Documents.Service.Interfaces;

public interface ILiveSessionService
{
    Task<object> CreateSessionAsync(long userId, string title, string? description, string sessionType,
        int? subjectId, int? topicId, int difficulty, int questionCount, int timeLimitMinutes, long? spaceId);
    Task<object?> GetSessionByIdAsync(long sessionId);
    Task<object?> GetSessionByInviteCodeAsync(string inviteCode);
    Task<IEnumerable<object>> GetActiveSessionsAsync();
    Task<IEnumerable<object>> GetMySessionsAsync(long userId);
    Task<IEnumerable<object>> GetSessionsBySpaceAsync(long spaceId);
    Task<bool> JoinSessionAsync(long sessionId, long userId);
    Task<bool> LeaveSessionAsync(long sessionId, long userId);
    Task<bool> StartSessionAsync(long sessionId, long userId);
    Task<bool> EndSessionAsync(long sessionId, long userId);
    Task<bool> SetReadyAsync(long sessionId, long userId, bool isReady);
    Task<object?> GetSessionMembersAsync(long sessionId);
    Task<object?> GetLeaderboardAsync(long sessionId);
    Task<bool> SubmitAnswerAsync(long sessionId, long userId, long questionId, long? optionId, char? letter, int timeSpent);
    Task<object?> GetCurrentQuestionAsync(long sessionId);
    Task<bool> SetCurrentQuestionAsync(long sessionId, long userId, long questionId);
    Task<object?> GetSessionActivitiesAsync(long sessionId, int limit = 50);
    Task<bool> SendChatMessageAsync(long sessionId, long userId, string content, string messageType = "text");
    Task<IEnumerable<object>> GetChatMessagesAsync(long sessionId, int limit = 100);
}

public interface IWhiteboardService
{
    Task<object?> GetWhiteboardItemsAsync(long sessionId);
    Task<object?> AddTextElementAsync(long sessionId, long userId, string text, int x, int y, string? color, int? fontSize);
    Task<object?> AddDrawingElementAsync(long sessionId, long userId, string drawingData, int x, int y, string color, int? width, int? height);
    Task<bool> UpdateElementAsync(long elementId, long userId, string content, int x, int y);
    Task<bool> DeleteElementAsync(long elementId, long userId);
    Task<bool> ClearWhiteboardAsync(long sessionId, long userId);
    Task<bool> LockElementAsync(long elementId, long userId, bool isLocked);
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository.Interfaces;

public interface ILiveSessionRepository
{
    Task<LiveStudySession?> GetByIdAsync(long id);
    Task<LiveStudySession?> GetByInviteCodeAsync(string inviteCode);
    Task<IEnumerable<LiveStudySession>> GetBySpaceIdAsync(long spaceId);
    Task<IEnumerable<LiveStudySession>> GetActiveSessionsAsync();
    Task<IEnumerable<LiveStudySession>> GetByUserIdAsync(long userId);
    Task<LiveStudySession> CreateAsync(LiveStudySession session);
    Task<LiveStudySession> UpdateAsync(LiveStudySession session);
    Task<bool> DeleteAsync(long id);
    Task<bool> IncrementParticipantsAsync(long id);
    Task<bool> DecrementParticipantsAsync(long id);
}

public interface ILiveSessionMemberRepository
{
    Task<LiveSessionMember?> GetByIdAsync(long id);
    Task<LiveSessionMember?> GetBySessionAndUserAsync(long sessionId, long userId);
    Task<IEnumerable<LiveSessionMember>> GetBySessionIdAsync(long sessionId);
    Task<IEnumerable<LiveSessionMember>> GetByUserIdAsync(long userId);
    Task<LiveSessionMember> CreateAsync(LiveSessionMember member);
    Task<LiveSessionMember> UpdateAsync(LiveSessionMember member);
    Task<bool> LeaveSessionAsync(long sessionId, long userId);
    Task<bool> UpdateScoreAsync(long memberId, int scoreToAdd, bool isCorrect);
    Task<bool> SetReadyAsync(long sessionId, long userId, bool isReady);
    Task<bool> UpdateStatusAsync(long sessionId, long userId, string status);
}

public interface ISessionActivityRepository
{
    Task<IEnumerable<SessionActivity>> GetBySessionIdAsync(long sessionId, int limit = 50);
    Task<SessionActivity> CreateAsync(SessionActivity activity);
}

public interface ISessionChatRepository
{
    Task<IEnumerable<SessionChatMessage>> GetBySessionIdAsync(long sessionId, int limit = 100);
    Task<SessionChatMessage> CreateAsync(SessionChatMessage message);
    Task<bool> PinMessageAsync(long messageId);
    Task<bool> UnpinMessageAsync(long messageId);
}

public interface ISessionWhiteboardRepository
{
    Task<IEnumerable<SessionWhiteboard>> GetBySessionIdAsync(long sessionId);
    Task<SessionWhiteboard?> GetByIdAsync(long id);
    Task<SessionWhiteboard> CreateAsync(SessionWhiteboard item);
    Task<SessionWhiteboard> UpdateAsync(SessionWhiteboard item);
    Task<bool> DeleteAsync(long id);
    Task<bool> DeleteBySessionAsync(long sessionId);
}

public interface ISessionSharedQuestionRepository
{
    Task<IEnumerable<SessionSharedQuestion>> GetBySessionIdAsync(long sessionId);
    Task<SessionSharedQuestion?> GetCurrentQuestionAsync(long sessionId);
    Task<SessionSharedQuestion> CreateAsync(SessionSharedQuestion sharedQuestion);
    Task<bool> SetCurrentQuestionAsync(long sessionId, long questionId);
    Task<bool> ClearCurrentQuestionAsync(long sessionId);
}

public interface ISessionParticipantAnswerRepository
{
    Task<SessionParticipantAnswer?> GetAsync(long sessionId, long userId, long questionId);
    Task<IEnumerable<SessionParticipantAnswer>> GetBySessionAsync(long sessionId);
    Task<IEnumerable<SessionParticipantAnswer>> GetByUserAsync(long sessionId, long userId);
    Task<SessionParticipantAnswer> CreateOrUpdateAsync(SessionParticipantAnswer answer);
}

public interface ISessionLeaderboardRepository
{
    Task<SessionLeaderboard?> GetAsync(long sessionId, long userId);
    Task<IEnumerable<SessionLeaderboard>> GetBySessionIdAsync(long sessionId);
    Task<SessionLeaderboard> CreateOrUpdateAsync(SessionLeaderboard entry);
    Task RecalculateRanksAsync(long sessionId);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository.Repositories;

public class LiveSessionRepository : ILiveSessionRepository
{
    private readonly BookstoreDbContext _context;

    public LiveSessionRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<LiveStudySession?> GetByIdAsync(long id)
    {
        return await _context.LiveStudySessions
            .Include(s => s.Host)
            .Include(s => s.Space)
            .Include(s => s.Subject)
            .Include(s => s.Members)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<LiveStudySession?> GetByInviteCodeAsync(string inviteCode)
    {
        return await _context.LiveStudySessions
            .Include(s => s.Host)
            .FirstOrDefaultAsync(s => s.InviteCode == inviteCode && s.Status != "completed");
    }

    public async Task<IEnumerable<LiveStudySession>> GetBySpaceIdAsync(long spaceId)
    {
        return await _context.LiveStudySessions
            .Where(s => s.SpaceId == spaceId && s.Status != "completed")
            .Include(s => s.Host)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LiveStudySession>> GetActiveSessionsAsync()
    {
        return await _context.LiveStudySessions
            .Where(s => s.Status == "waiting" || s.Status == "in_progress")
            .Include(s => s.Host)
            .Include(s => s.Space)
            .OrderByDescending(s => s.CreatedAt)
            .Take(50)
            .ToListAsync();
    }

    public async Task<IEnumerable<LiveStudySession>> GetByUserIdAsync(long userId)
    {
        var sessionIds = await _context.LiveSessionMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.SessionId)
            .ToListAsync();

        return await _context.LiveStudySessions
            .Where(s => sessionIds.Contains(s.Id))
            .Include(s => s.Host)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<LiveStudySession> CreateAsync(LiveStudySession session)
    {
        session.CreatedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        session.InviteCode = GenerateInviteCode();

        _context.LiveStudySessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<LiveStudySession> UpdateAsync(LiveStudySession session)
    {
        session.UpdatedAt = DateTime.UtcNow;
        _context.LiveStudySessions.Update(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var session = await _context.LiveStudySessions.FindAsync(id);
        if (session == null) return false;

        _context.LiveStudySessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IncrementParticipantsAsync(long id)
    {
        var session = await _context.LiveStudySessions.FindAsync(id);
        if (session == null) return false;

        session.CurrentParticipants++;
        if (session.CurrentParticipants >= session.MaxParticipants)
        {
            session.Status = "full";
        }
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DecrementParticipantsAsync(long id)
    {
        var session = await _context.LiveStudySessions.FindAsync(id);
        if (session == null) return false;

        session.CurrentParticipants = Math.Max(0, session.CurrentParticipants - 1);
        await _context.SaveChangesAsync();
        return true;
    }

    private string GenerateInviteCode()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpper();
    }
}

public class LiveSessionMemberRepository : ILiveSessionMemberRepository
{
    private readonly BookstoreDbContext _context;

    public LiveSessionMemberRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<LiveSessionMember?> GetByIdAsync(long id)
    {
        return await _context.LiveSessionMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<LiveSessionMember?> GetBySessionAndUserAsync(long sessionId, long userId)
    {
        return await _context.LiveSessionMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.SessionId == sessionId && m.UserId == userId);
    }

    public async Task<IEnumerable<LiveSessionMember>> GetBySessionIdAsync(long sessionId)
    {
        return await _context.LiveSessionMembers
            .Include(m => m.User)
            .Where(m => m.SessionId == sessionId && m.Status != "left")
            .OrderByDescending(m => m.TotalScore)
            .ToListAsync();
    }

    public async Task<IEnumerable<LiveSessionMember>> GetByUserIdAsync(long userId)
    {
        return await _context.LiveSessionMembers
            .Include(m => m.Session)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.JoinedAt)
            .ToListAsync();
    }

    public async Task<LiveSessionMember> CreateAsync(LiveSessionMember member)
    {
        member.JoinedAt = DateTime.UtcNow;
        member.LastActivityAt = DateTime.UtcNow;

        _context.LiveSessionMembers.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task<LiveSessionMember> UpdateAsync(LiveSessionMember member)
    {
        member.LastActivityAt = DateTime.UtcNow;
        _context.LiveSessionMembers.Update(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task<bool> LeaveSessionAsync(long sessionId, long userId)
    {
        var member = await GetBySessionAndUserAsync(sessionId, userId);
        if (member == null) return false;

        member.Status = "left";
        member.LeftAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Decrement participant count
        var session = await _context.LiveStudySessions.FindAsync(sessionId);
        if (session != null)
        {
            session.CurrentParticipants = Math.Max(0, session.CurrentParticipants - 1);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> UpdateScoreAsync(long memberId, int scoreToAdd, bool isCorrect)
    {
        var member = await _context.LiveSessionMembers.FindAsync(memberId);
        if (member == null) return false;

        member.TotalScore += scoreToAdd;
        member.QuestionsAnswered++;
        if (isCorrect)
        {
            member.CorrectAnswers++;
            member.CurrentStreak++;
        }
        else
        {
            member.CurrentStreak = 0;
        }
        member.LastActivityAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetReadyAsync(long sessionId, long userId, bool isReady)
    {
        var member = await GetBySessionAndUserAsync(sessionId, userId);
        if (member == null) return false;

        member.IsReady = isReady;
        member.LastActivityAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(long sessionId, long userId, string status)
    {
        var member = await GetBySessionAndUserAsync(sessionId, userId);
        if (member == null) return false;

        member.Status = status;
        member.LastActivityAt = DateTime.UtcNow;
        if (status == "left")
        {
            member.LeftAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
        return true;
    }
}

public class SessionActivityRepository : ISessionActivityRepository
{
    private readonly BookstoreDbContext _context;

    public SessionActivityRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SessionActivity>> GetBySessionIdAsync(long sessionId, int limit = 50)
    {
        return await _context.SessionActivities
            .Include(a => a.User)
            .Where(a => a.SessionId == sessionId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<SessionActivity> CreateAsync(SessionActivity activity)
    {
        activity.CreatedAt = DateTime.UtcNow;
        _context.SessionActivities.Add(activity);
        await _context.SaveChangesAsync();
        return activity;
    }
}

public class SessionChatRepository : ISessionChatRepository
{
    private readonly BookstoreDbContext _context;

    public SessionChatRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SessionChatMessage>> GetBySessionIdAsync(long sessionId, int limit = 100)
    {
        return await _context.SessionChatMessages
            .Include(m => m.User)
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<SessionChatMessage> CreateAsync(SessionChatMessage message)
    {
        message.CreatedAt = DateTime.UtcNow;
        _context.SessionChatMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<bool> PinMessageAsync(long messageId)
    {
        var message = await _context.SessionChatMessages.FindAsync(messageId);
        if (message == null) return false;

        message.IsPinned = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnpinMessageAsync(long messageId)
    {
        var message = await _context.SessionChatMessages.FindAsync(messageId);
        if (message == null) return false;

        message.IsPinned = false;
        await _context.SaveChangesAsync();
        return true;
    }
}

public class SessionWhiteboardRepository : ISessionWhiteboardRepository
{
    private readonly BookstoreDbContext _context;

    public SessionWhiteboardRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SessionWhiteboard>> GetBySessionIdAsync(long sessionId)
    {
        return await _context.SessionWhiteboards
            .Include(w => w.User)
            .Where(w => w.SessionId == sessionId)
            .OrderBy(w => w.LayerIndex)
            .ToListAsync();
    }

    public async Task<SessionWhiteboard?> GetByIdAsync(long id)
    {
        return await _context.SessionWhiteboards.FindAsync(id);
    }

    public async Task<SessionWhiteboard> CreateAsync(SessionWhiteboard item)
    {
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        _context.SessionWhiteboards.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<SessionWhiteboard> UpdateAsync(SessionWhiteboard item)
    {
        item.UpdatedAt = DateTime.UtcNow;
        _context.SessionWhiteboards.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var item = await _context.SessionWhiteboards.FindAsync(id);
        if (item == null) return false;

        _context.SessionWhiteboards.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBySessionAsync(long sessionId)
    {
        var items = await _context.SessionWhiteboards
            .Where(w => w.SessionId == sessionId)
            .ToListAsync();

        _context.SessionWhiteboards.RemoveRange(items);
        await _context.SaveChangesAsync();
        return true;
    }
}

public class SessionSharedQuestionRepository : ISessionSharedQuestionRepository
{
    private readonly BookstoreDbContext _context;

    public SessionSharedQuestionRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SessionSharedQuestion>> GetBySessionIdAsync(long sessionId)
    {
        return await _context.SessionSharedQuestions
            .Include(q => q.Question)
            .ThenInclude(q => q.QuestionOptions)
            .Where(q => q.SessionId == sessionId)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync();
    }

    public async Task<SessionSharedQuestion?> GetCurrentQuestionAsync(long sessionId)
    {
        return await _context.SessionSharedQuestions
            .Include(q => q.Question)
            .ThenInclude(q => q.QuestionOptions)
            .FirstOrDefaultAsync(q => q.SessionId == sessionId && q.IsCurrent);
    }

    public async Task<SessionSharedQuestion> CreateAsync(SessionSharedQuestion sharedQuestion)
    {
        sharedQuestion.SharedAt = DateTime.UtcNow;
        _context.SessionSharedQuestions.Add(sharedQuestion);
        await _context.SaveChangesAsync();
        return sharedQuestion;
    }

    public async Task<bool> SetCurrentQuestionAsync(long sessionId, long questionId)
    {
        // Clear all current flags
        var currentQuestions = await _context.SessionSharedQuestions
            .Where(q => q.SessionId == sessionId && q.IsCurrent)
            .ToListAsync();

        foreach (var q in currentQuestions)
        {
            q.IsCurrent = false;
        }

        // Set the new current question
        var question = await _context.SessionSharedQuestions
            .FirstOrDefaultAsync(q => q.SessionId == sessionId && q.QuestionId == questionId);

        if (question != null)
        {
            question.IsCurrent = true;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> ClearCurrentQuestionAsync(long sessionId)
    {
        var currentQuestions = await _context.SessionSharedQuestions
            .Where(q => q.SessionId == sessionId && q.IsCurrent)
            .ToListAsync();

        foreach (var q in currentQuestions)
        {
            q.IsCurrent = false;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}

public class SessionParticipantAnswerRepository : ISessionParticipantAnswerRepository
{
    private readonly BookstoreDbContext _context;

    public SessionParticipantAnswerRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<SessionParticipantAnswer?> GetAsync(long sessionId, long userId, long questionId)
    {
        return await _context.SessionParticipantAnswers
            .Include(a => a.Question)
            .FirstOrDefaultAsync(a => a.SessionId == sessionId && a.UserId == userId && a.QuestionId == questionId);
    }

    public async Task<IEnumerable<SessionParticipantAnswer>> GetBySessionAsync(long sessionId)
    {
        return await _context.SessionParticipantAnswers
            .Include(a => a.User)
            .Include(a => a.Question)
            .Where(a => a.SessionId == sessionId)
            .OrderByDescending(a => a.AnsweredAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SessionParticipantAnswer>> GetByUserAsync(long sessionId, long userId)
    {
        return await _context.SessionParticipantAnswers
            .Include(a => a.Question)
            .Where(a => a.SessionId == sessionId && a.UserId == userId)
            .OrderByDescending(a => a.AnsweredAt)
            .ToListAsync();
    }

    public async Task<SessionParticipantAnswer> CreateOrUpdateAsync(SessionParticipantAnswer answer)
    {
        var existing = await GetAsync(answer.SessionId, answer.UserId, answer.QuestionId);

        if (existing != null)
        {
            existing.SelectedOptionId = answer.SelectedOptionId;
            existing.SelectedLetter = answer.SelectedLetter;
            existing.AnswerText = answer.AnswerText;
            existing.IsCorrect = answer.IsCorrect;
            existing.TimeSpentSeconds = answer.TimeSpentSeconds;
            existing.PointsEarned = answer.PointsEarned;
            existing.AnsweredAt = DateTime.UtcNow;

            _context.SessionParticipantAnswers.Update(existing);
        }
        else
        {
            answer.AnsweredAt = DateTime.UtcNow;
            _context.SessionParticipantAnswers.Add(answer);
        }

        await _context.SaveChangesAsync();
        return existing ?? answer;
    }
}

public class SessionLeaderboardRepository : ISessionLeaderboardRepository
{
    private readonly BookstoreDbContext _context;

    public SessionLeaderboardRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<SessionLeaderboard?> GetAsync(long sessionId, long userId)
    {
        return await _context.SessionLeaderboards
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.SessionId == sessionId && l.UserId == userId);
    }

    public async Task<IEnumerable<SessionLeaderboard>> GetBySessionIdAsync(long sessionId)
    {
        return await _context.SessionLeaderboards
            .Include(l => l.User)
            .Where(l => l.SessionId == sessionId)
            .OrderByDescending(l => l.TotalScore)
            .ToListAsync();
    }

    public async Task<SessionLeaderboard> CreateOrUpdateAsync(SessionLeaderboard entry)
    {
        var existing = await GetAsync(entry.SessionId, entry.UserId);

        if (existing != null)
        {
            existing.TotalScore = entry.TotalScore;
            existing.QuestionsCorrect = entry.QuestionsCorrect;
            existing.TotalQuestions = entry.TotalQuestions;
            existing.AverageTimeSeconds = entry.AverageTimeSeconds;
            existing.FastestAnswerSeconds = entry.FastestAnswerSeconds;
            existing.LongestStreak = entry.LongestStreak;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.SessionLeaderboards.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        entry.UpdatedAt = DateTime.UtcNow;
        _context.SessionLeaderboards.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task RecalculateRanksAsync(long sessionId)
    {
        var leaderboard = await _context.SessionLeaderboards
            .Where(l => l.SessionId == sessionId)
            .OrderByDescending(l => l.TotalScore)
            .ToListAsync();

        int rank = 1;
        foreach (var entry in leaderboard)
        {
            entry.RankPosition = rank++;
            _context.SessionLeaderboards.Update(entry);
        }

        await _context.SaveChangesAsync();
    }
}

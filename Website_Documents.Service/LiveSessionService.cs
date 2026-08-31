using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class LiveSessionService : ILiveSessionService
{
    private readonly ILiveSessionRepository _sessionRepository;
    private readonly ILiveSessionMemberRepository _memberRepository;
    private readonly ISessionActivityRepository _activityRepository;
    private readonly ISessionChatRepository _chatRepository;
    private readonly ISessionSharedQuestionRepository _sharedQuestionRepository;
    private readonly ISessionParticipantAnswerRepository _answerRepository;
    private readonly ISessionLeaderboardRepository _leaderboardRepository;

    public LiveSessionService(
        ILiveSessionRepository sessionRepository,
        ILiveSessionMemberRepository memberRepository,
        ISessionActivityRepository activityRepository,
        ISessionChatRepository chatRepository,
        ISessionSharedQuestionRepository sharedQuestionRepository,
        ISessionParticipantAnswerRepository answerRepository,
        ISessionLeaderboardRepository leaderboardRepository)
    {
        _sessionRepository = sessionRepository;
        _memberRepository = memberRepository;
        _activityRepository = activityRepository;
        _chatRepository = chatRepository;
        _sharedQuestionRepository = sharedQuestionRepository;
        _answerRepository = answerRepository;
        _leaderboardRepository = leaderboardRepository;
    }

    public async Task<object> CreateSessionAsync(long userId, string title, string? description, string sessionType,
        int? subjectId, int? topicId, int difficulty, int questionCount, int timeLimitMinutes, long? spaceId)
    {
        var session = new LiveStudySession
        {
            SpaceId = spaceId,
            Title = title,
            Description = description,
            SessionType = sessionType,
            SubjectId = subjectId,
            TopicId = topicId,
            DifficultyLevel = (short)difficulty,
            QuestionCount = questionCount,
            TimeLimitMinutes = timeLimitMinutes,
            Status = "waiting",
            HostId = userId,
            CurrentParticipants = 1
        };

        var created = await _sessionRepository.CreateAsync(session);

        // Add host as member
        var member = new LiveSessionMember
        {
            SessionId = created.Id,
            UserId = userId,
            Role = "host",
            Status = "joined",
            IsReady = true
        };
        await _memberRepository.CreateAsync(member);

        // Log activity
        await _activityRepository.CreateAsync(new SessionActivity
        {
            SessionId = created.Id,
            UserId = userId,
            ActivityType = "session_created",
            Description = $"Session '{title}' was created"
        });

        return new
        {
            id = created.Id,
            title = created.Title,
            description = created.Description,
            sessionType = created.SessionType,
            inviteCode = created.InviteCode,
            status = created.Status,
            difficultyLevel = created.DifficultyLevel,
            questionCount = created.QuestionCount,
            timeLimitMinutes = created.TimeLimitMinutes,
            maxParticipants = created.MaxParticipants,
            currentParticipants = created.CurrentParticipants,
            hostId = created.HostId,
            createdAt = created.CreatedAt
        };
    }

    public async Task<object?> GetSessionByIdAsync(long sessionId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return null;

        return MapSessionToDto(session);
    }

    public async Task<object?> GetSessionByInviteCodeAsync(string inviteCode)
    {
        var session = await _sessionRepository.GetByInviteCodeAsync(inviteCode);
        if (session == null) return null;

        return MapSessionToDto(session);
    }

    public async Task<IEnumerable<object>> GetActiveSessionsAsync()
    {
        var sessions = await _sessionRepository.GetActiveSessionsAsync();
        return sessions.Select(MapSessionToDto);
    }

    public async Task<IEnumerable<object>> GetMySessionsAsync(long userId)
    {
        var sessions = await _sessionRepository.GetByUserIdAsync(userId);
        return sessions.Select(MapSessionToDto);
    }

    public async Task<IEnumerable<object>> GetSessionsBySpaceAsync(long spaceId)
    {
        var sessions = await _sessionRepository.GetBySpaceIdAsync(spaceId);
        return sessions.Select(MapSessionToDto);
    }

    public async Task<bool> JoinSessionAsync(long sessionId, long userId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return false;

        if (session.CurrentParticipants >= session.MaxParticipants)
            return false;

        if (session.Status == "completed" || session.Status == "cancelled")
            return false;

        // Check if already a member
        var existingMember = await _memberRepository.GetBySessionAndUserAsync(sessionId, userId);
        if (existingMember != null)
        {
            if (existingMember.Status == "left")
            {
                existingMember.Status = "joined";
                existingMember.JoinedAt = DateTime.UtcNow;
                await _memberRepository.UpdateAsync(existingMember);
                await _sessionRepository.IncrementParticipantsAsync(sessionId);
            }
            return true;
        }

        // Add new member
        var member = new LiveSessionMember
        {
            SessionId = sessionId,
            UserId = userId,
            Role = "participant",
            Status = "joined",
            IsReady = false
        };
        await _memberRepository.CreateAsync(member);
        await _sessionRepository.IncrementParticipantsAsync(sessionId);

        // Log activity
        await _activityRepository.CreateAsync(new SessionActivity
        {
            SessionId = sessionId,
            UserId = userId,
            ActivityType = "user_joined",
            Description = "User joined the session"
        });

        return true;
    }

    public async Task<bool> LeaveSessionAsync(long sessionId, long userId)
    {
        var member = await _memberRepository.GetBySessionAndUserAsync(sessionId, userId);
        if (member == null) return false;

        if (member.Role == "host")
            return false; // Host cannot leave

        await _memberRepository.LeaveSessionAsync(sessionId, userId);

        // Log activity
        await _activityRepository.CreateAsync(new SessionActivity
        {
            SessionId = sessionId,
            UserId = userId,
            ActivityType = "user_left",
            Description = "User left the session"
        });

        return true;
    }

    public async Task<bool> StartSessionAsync(long sessionId, long userId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return false;

        if (session.HostId != userId)
            return false;

        if (session.Status != "waiting")
            return false;

        session.Status = "in_progress";
        session.StartedAt = DateTime.UtcNow;
        await _sessionRepository.UpdateAsync(session);

        // Log activity
        await _activityRepository.CreateAsync(new SessionActivity
        {
            SessionId = sessionId,
            UserId = userId,
            ActivityType = "session_started",
            Description = "Session started"
        });

        return true;
    }

    public async Task<bool> EndSessionAsync(long sessionId, long userId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return false;

        if (session.HostId != userId)
            return false;

        session.Status = "completed";
        session.EndedAt = DateTime.UtcNow;
        await _sessionRepository.UpdateAsync(session);

        // Recalculate final leaderboard
        await _leaderboardRepository.RecalculateRanksAsync(sessionId);

        // Log activity
        await _activityRepository.CreateAsync(new SessionActivity
        {
            SessionId = sessionId,
            UserId = userId,
            ActivityType = "session_ended",
            Description = "Session ended"
        });

        return true;
    }

    public async Task<bool> SetReadyAsync(long sessionId, long userId, bool isReady)
    {
        return await _memberRepository.SetReadyAsync(sessionId, userId, isReady);
    }

    public async Task<object?> GetSessionMembersAsync(long sessionId)
    {
        var members = await _memberRepository.GetBySessionIdAsync(sessionId);
        return members.Select(m => new
        {
            id = m.Id,
            userId = m.UserId,
            userName = m.User?.FullName,
            userAvatar = m.User?.AvatarUrl,
            role = m.Role,
            status = m.Status,
            isReady = m.IsReady,
            questionsAnswered = m.QuestionsAnswered,
            correctAnswers = m.CorrectAnswers,
            totalScore = m.TotalScore,
            currentStreak = m.CurrentStreak,
            joinedAt = m.JoinedAt
        });
    }

    public async Task<object?> GetLeaderboardAsync(long sessionId)
    {
        var leaderboard = await _leaderboardRepository.GetBySessionIdAsync(sessionId);
        return leaderboard.Select(l => new
        {
            rank = l.RankPosition,
            userId = l.UserId,
            userName = l.User?.FullName,
            userAvatar = l.User?.AvatarUrl,
            totalScore = l.TotalScore,
            questionsCorrect = l.QuestionsCorrect,
            totalQuestions = l.TotalQuestions,
            averageTimeSeconds = l.AverageTimeSeconds,
            fastestAnswerSeconds = l.FastestAnswerSeconds,
            longestStreak = l.LongestStreak
        });
    }

    public async Task<bool> SubmitAnswerAsync(long sessionId, long userId, long questionId, long? optionId, char? letter, int timeSpent)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return false;

        if (session.Status != "in_progress")
            return false;

        // Get member
        var member = await _memberRepository.GetBySessionAndUserAsync(sessionId, userId);
        if (member == null) return false;

        // Create or update answer
        var answer = new SessionParticipantAnswer
        {
            SessionId = sessionId,
            UserId = userId,
            QuestionId = questionId,
            SelectedOptionId = optionId,
            SelectedLetter = letter,
            TimeSpentSeconds = timeSpent,
            AnsweredAt = DateTime.UtcNow
        };

        await _answerRepository.CreateOrUpdateAsync(answer);

        // Update member score
        int points = 100 - (timeSpent / 2); // Points based on speed
        bool isCorrect = optionId.HasValue; // Simplified check
        await _memberRepository.UpdateScoreAsync(member.Id, points, isCorrect);

        // Update leaderboard
        var leaderboardEntry = await _leaderboardRepository.GetAsync(sessionId, userId);
        if (leaderboardEntry == null)
        {
            leaderboardEntry = new SessionLeaderboard
            {
                SessionId = sessionId,
                UserId = userId,
                TotalScore = member.TotalScore + points,
                QuestionsCorrect = isCorrect ? 1 : 0,
                TotalQuestions = 1,
                AverageTimeSeconds = timeSpent,
                FastestAnswerSeconds = timeSpent,
                LongestStreak = isCorrect ? 1 : 0
            };
        }
        else
        {
            leaderboardEntry.TotalScore = member.TotalScore;
            leaderboardEntry.QuestionsCorrect = member.CorrectAnswers;
            leaderboardEntry.TotalQuestions = member.QuestionsAnswered;
            leaderboardEntry.AverageTimeSeconds = (leaderboardEntry.AverageTimeSeconds * (leaderboardEntry.TotalQuestions - 1) + timeSpent) / leaderboardEntry.TotalQuestions;
            if (timeSpent < (leaderboardEntry.FastestAnswerSeconds ?? int.MaxValue))
                leaderboardEntry.FastestAnswerSeconds = timeSpent;
            if (isCorrect && member.CurrentStreak > leaderboardEntry.LongestStreak)
                leaderboardEntry.LongestStreak = member.CurrentStreak;
        }
        await _leaderboardRepository.CreateOrUpdateAsync(leaderboardEntry);

        // Log activity
        await _activityRepository.CreateAsync(new SessionActivity
        {
            SessionId = sessionId,
            UserId = userId,
            ActivityType = "answer_submitted",
            Description = $"User submitted answer for question {questionId}"
        });

        return true;
    }

    public async Task<object?> GetCurrentQuestionAsync(long sessionId)
    {
        var currentQuestion = await _sharedQuestionRepository.GetCurrentQuestionAsync(sessionId);
        if (currentQuestion == null) return null;

        return new
        {
            id = currentQuestion.Id,
            questionId = currentQuestion.QuestionId,
            question = new
            {
                id = currentQuestion.Question?.Id,
                content = currentQuestion.Question?.Content,
                questionType = currentQuestion.Question?.QuestionType,
                options = currentQuestion.Question?.QuestionOptions.Select(o => new
                {
                    id = o.Id,
                    content = o.Content,
                    orderIndex = o.OrderIndex
                })
            },
            sharedAt = currentQuestion.SharedAt
        };
    }

    public async Task<bool> SetCurrentQuestionAsync(long sessionId, long userId, long questionId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return false;

        if (session.HostId != userId)
            return false;

        // Share question in session
        var sharedQuestion = new SessionSharedQuestion
        {
            SessionId = sessionId,
            QuestionId = questionId,
            SharedBy = userId,
            SharedAt = DateTime.UtcNow,
            IsCurrent = true
        };
        await _sharedQuestionRepository.CreateAsync(sharedQuestion);

        // Clear other current flags
        await _sharedQuestionRepository.ClearCurrentQuestionAsync(sessionId);
        await _sharedQuestionRepository.SetCurrentQuestionAsync(sessionId, questionId);

        // Log activity
        await _activityRepository.CreateAsync(new SessionActivity
        {
            SessionId = sessionId,
            UserId = userId,
            ActivityType = "question_shown",
            Description = $"Question {questionId} is now the current question"
        });

        return true;
    }

    public async Task<object?> GetSessionActivitiesAsync(long sessionId, int limit = 50)
    {
        var activities = await _activityRepository.GetBySessionIdAsync(sessionId, limit);
        return activities.Select(a => new
        {
            id = a.Id,
            userId = a.UserId,
            userName = a.User?.FullName,
            activityType = a.ActivityType,
            description = a.Description,
            metadata = a.Metadata,
            createdAt = a.CreatedAt
        });
    }

    public async Task<bool> SendChatMessageAsync(long sessionId, long userId, string content, string messageType = "text")
    {
        var message = new SessionChatMessage
        {
            SessionId = sessionId,
            UserId = userId,
            Content = content,
            MessageType = messageType
        };
        await _chatRepository.CreateAsync(message);
        return true;
    }

    public async Task<IEnumerable<object>> GetChatMessagesAsync(long sessionId, int limit = 100)
    {
        var messages = await _chatRepository.GetBySessionIdAsync(sessionId, limit);
        return messages.Select(m => new
        {
            id = m.Id,
            userId = m.UserId,
            userName = m.User?.FullName,
            userAvatar = m.User?.AvatarUrl,
            content = m.Content,
            messageType = m.MessageType,
            replyToId = m.ReplyToId,
            isPinned = m.IsPinned,
            createdAt = m.CreatedAt
        });
    }

    private object MapSessionToDto(LiveStudySession session)
    {
        return new
        {
            id = session.Id,
            spaceId = session.SpaceId,
            spaceName = session.Space?.Name,
            title = session.Title,
            description = session.Description,
            sessionType = session.SessionType,
            subjectId = session.SubjectId,
            subjectName = session.Subject?.Name,
            topicId = session.TopicId,
            topicName = session.Topic?.Name,
            difficultyLevel = session.DifficultyLevel,
            questionCount = session.QuestionCount,
            timeLimitMinutes = session.TimeLimitMinutes,
            status = session.Status,
            maxParticipants = session.MaxParticipants,
            currentParticipants = session.CurrentParticipants,
            inviteCode = session.InviteCode,
            hostId = session.HostId,
            hostName = session.Host?.FullName,
            hostAvatar = session.Host?.AvatarUrl,
            startedAt = session.StartedAt,
            endedAt = session.EndedAt,
            createdAt = session.CreatedAt
        };
    }
}

public class WhiteboardService : IWhiteboardService
{
    private readonly ISessionWhiteboardRepository _whiteboardRepository;
    private readonly ILiveSessionRepository _sessionRepository;

    public WhiteboardService(
        ISessionWhiteboardRepository whiteboardRepository,
        ILiveSessionRepository sessionRepository)
    {
        _whiteboardRepository = whiteboardRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<object?> GetWhiteboardItemsAsync(long sessionId)
    {
        var items = await _whiteboardRepository.GetBySessionIdAsync(sessionId);
        return items.Select(i => new
        {
            id = i.Id,
            userId = i.UserId,
            userName = i.User?.FullName,
            elementType = i.ElementType,
            content = i.Content,
            positionX = i.PositionX,
            positionY = i.PositionY,
            width = i.Width,
            height = i.Height,
            color = i.Color,
            fontSize = i.FontSize,
            layerIndex = i.LayerIndex,
            isLocked = i.IsLocked,
            createdAt = i.CreatedAt
        });
    }

    public async Task<object?> AddTextElementAsync(long sessionId, long userId, string text, int x, int y, string? color, int? fontSize)
    {
        var item = new SessionWhiteboard
        {
            SessionId = sessionId,
            UserId = userId,
            ElementType = "text",
            Content = text,
            PositionX = x,
            PositionY = y,
            Color = color ?? "#000000",
            FontSize = fontSize ?? 16
        };
        var created = await _whiteboardRepository.CreateAsync(item);
        return created;
    }

    public async Task<object?> AddDrawingElementAsync(long sessionId, long userId, string drawingData, int x, int y, string color, int? width, int? height)
    {
        var item = new SessionWhiteboard
        {
            SessionId = sessionId,
            UserId = userId,
            ElementType = "drawing",
            Content = drawingData,
            PositionX = x,
            PositionY = y,
            Color = color,
            Width = width,
            Height = height
        };
        var created = await _whiteboardRepository.CreateAsync(item);
        return created;
    }

    public async Task<bool> UpdateElementAsync(long elementId, long userId, string content, int x, int y)
    {
        var element = await _whiteboardRepository.GetByIdAsync(elementId);
        if (element == null) return false;
        if (element.UserId != userId) return false;

        element.Content = content;
        element.PositionX = x;
        element.PositionY = y;
        await _whiteboardRepository.UpdateAsync(element);
        return true;
    }

    public async Task<bool> DeleteElementAsync(long elementId, long userId)
    {
        var element = await _whiteboardRepository.GetByIdAsync(elementId);
        if (element == null) return false;
        if (element.UserId != userId) return false;

        return await _whiteboardRepository.DeleteAsync(elementId);
    }

    public async Task<bool> ClearWhiteboardAsync(long sessionId, long userId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return false;
        if (session.HostId != userId) return false;

        return await _whiteboardRepository.DeleteBySessionAsync(sessionId);
    }

    public async Task<bool> LockElementAsync(long elementId, long userId, bool isLocked)
    {
        var element = await _whiteboardRepository.GetByIdAsync(elementId);
        if (element == null) return false;

        element.IsLocked = isLocked;
        await _whiteboardRepository.UpdateAsync(element);
        return true;
    }
}

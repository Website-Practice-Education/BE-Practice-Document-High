using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class StudyService : IStudyService
{
    private readonly BookstoreDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public StudyService(BookstoreDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    // ===== Study Session =====

    public async Task<StudySession> StartStudySessionAsync(long userId, int subjectId)
    {
        var session = new StudySession
        {
            UserId = userId,
            SubjectId = subjectId,
            StartedAt = DateTime.UtcNow,
            Status = "active"
        };

        return session;
    }

    public async Task<StudySession?> GetStudySessionAsync(long sessionId)
    {
        var session = await _context.StudySessions.FindAsync(sessionId);
        if (session == null) return null;

        return new StudySession
        {
            Id = session.Id,
            UserId = session.UserId,
            SubjectId = session.SubjectId,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            QuestionsAnswered = session.QuestionsAnswered,
            CorrectAnswers = session.CorrectAnswers,
            TimeSpentMinutes = session.TimeSpentMinutes,
            Status = session.Status
        };
    }

    public async Task<StudySession?> GetActiveSessionAsync(long userId)
    {
        var session = await _context.StudySessions
            .Where(s => s.UserId == userId && s.Status == "active")
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync();

        if (session == null) return null;

        return new StudySession
        {
            Id = session.Id,
            UserId = session.UserId,
            SubjectId = session.SubjectId,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            QuestionsAnswered = session.QuestionsAnswered,
            CorrectAnswers = session.CorrectAnswers,
            TimeSpentMinutes = session.TimeSpentMinutes,
            Status = session.Status
        };
    }

    public async Task<StudySession> EndStudySessionAsync(long sessionId)
    {
        var session = await _context.StudySessions.FindAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException("Session not found");

        session.EndedAt = DateTime.UtcNow;
        session.Status = "completed";
        await _context.SaveChangesAsync();

        return new StudySession
        {
            Id = session.Id,
            UserId = session.UserId,
            SubjectId = session.SubjectId,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            QuestionsAnswered = session.QuestionsAnswered,
            CorrectAnswers = session.CorrectAnswers,
            TimeSpentMinutes = session.TimeSpentMinutes,
            Status = session.Status
        };
    }

    public async Task UpdateStudySessionProgressAsync(long sessionId, int questionsAnswered, int correctAnswers, int timeSpentMinutes)
    {
        var session = await _context.StudySessions.FindAsync(sessionId);
        if (session != null)
        {
            session.QuestionsAnswered = questionsAnswered;
            session.CorrectAnswers = correctAnswers;
            session.TimeSpentMinutes = timeSpentMinutes;
            await _context.SaveChangesAsync();
        }
    }

    // ===== Practice Questions =====

    public async Task<List<Question>> GetPracticeQuestionsAsync(long userId, int subjectId, int topicId, int count, short? minDifficulty, short? maxDifficulty)
    {
        var query = _context.Questions
            .Include(q => q.QuestionOptions)
            .Where(q => q.SubjectId == subjectId && q.IsActive == true);

        if (topicId > 0)
            query = query.Where(q => q.TopicId == topicId);

        if (minDifficulty.HasValue)
            query = query.Where(q => q.Difficulty >= minDifficulty.Value);

        if (maxDifficulty.HasValue)
            query = query.Where(q => q.Difficulty <= maxDifficulty.Value);

        var answeredQuestionIds = await _context.UserAnswerHistories
            .Where(h => h.UserId == userId)
            .Select(h => h.QuestionId)
            .Distinct()
            .ToListAsync();

        var questions = await query
            .Where(q => !answeredQuestionIds.Contains(q.Id))
            .OrderBy(q => Guid.NewGuid())
            .Take(count)
            .ToListAsync();

        return questions;
    }

    public async Task<List<Question>> GetWeakQuestionsAsync(long userId, int count)
    {
        var weakQuestionIds = await _context.UserAnswerHistories
            .Where(h => h.UserId == userId)
            .GroupBy(h => h.QuestionId)
            .Select(g => new
            {
                QuestionId = g.Key,
                TotalAttempts = g.Count(),
                CorrectAttempts = g.Count(a => a.IsCorrect)
            })
            .Where(x => x.TotalAttempts >= 2 && (decimal)x.CorrectAttempts / x.TotalAttempts < 0.5m)
            .OrderBy(x => (decimal)x.CorrectAttempts / x.TotalAttempts)
            .Take(count * 2)
            .Select(x => x.QuestionId)
            .ToListAsync();

        var questions = await _context.Questions
            .Include(q => q.QuestionOptions)
            .Where(q => weakQuestionIds.Contains(q.Id))
            .Take(count)
            .ToListAsync();

        return questions;
    }

    public async Task<List<Question>> GetRecommendedQuestionsAsync(long userId, int count)
    {
        var recommendedIds = new List<long>();

        var weakQuestions = await GetWeakQuestionsAsync(userId, count / 2);
        recommendedIds.AddRange(weakQuestions.Select(q => q.Id));

        var notAnswered = await _context.Questions
            .Where(q => !_context.UserAnswerHistories.Any(h => h.UserId == userId && h.QuestionId == q.Id))
            .Where(q => q.IsActive == true)
            .OrderBy(q => q.Difficulty)
            .Take(count / 2)
            .Select(q => q.Id)
            .ToListAsync();

        recommendedIds.AddRange(notAnswered);

        var questions = await _context.Questions
            .Include(q => q.QuestionOptions)
            .Where(q => recommendedIds.Contains(q.Id))
            .ToListAsync();

        return questions.Take(count).ToList();
    }

    // ===== Quiz Mode =====

    public async Task<QuizResult> StartQuizAsync(long userId, int subjectId, int questionCount, string difficulty)
    {
        var session = await StartStudySessionAsync(userId, subjectId);

        short? minDiff = difficulty.ToLower() switch
        {
            "easy" => (short)1,
            "medium" => (short)3,
            "hard" => (short)5,
            _ => null
        };

        var questions = await _context.Questions
            .Where(q => q.SubjectId == subjectId && q.IsActive == true)
            .Where(q => !minDiff.HasValue || q.Difficulty >= minDiff.Value)
            .OrderBy(q => Guid.NewGuid())
            .Take(questionCount)
            .ToListAsync();

        return new QuizResult
        {
            SessionId = session.Id,
            TotalQuestions = questions.Count,
            QuestionResults = new List<QuestionResult>()
        };
    }

    public async Task<bool> SubmitQuizAnswerAsync(long sessionId, long questionId, long? selectedOptionId, bool isCorrect, int timeSpentSeconds)
    {
        var answerHistory = new UserAnswerHistory
        {
            AttemptId = sessionId,
            QuestionId = questionId,
            SelectedOptionId = selectedOptionId,
            ChangedAt = DateTime.UtcNow,
            IsCorrect = isCorrect
        };

        _context.UserAnswerHistories.Add(answerHistory);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<QuizResult> CompleteQuizAsync(long sessionId)
    {
        var session = await EndStudySessionAsync(sessionId);

        var answers = await _context.UserAnswerHistories
            .Where(h => h.AttemptId == sessionId)
            .ToListAsync();

        var totalTime = answers.Sum(a => 60);

        return new QuizResult
        {
            SessionId = session.Id,
            TotalQuestions = answers.Count,
            CorrectAnswers = answers.Count(a => a.IsCorrect),
            TimeSpentMinutes = totalTime / 60,
            ScorePercentage = answers.Count > 0 
                ? Math.Round((decimal)answers.Count(a => a.IsCorrect) / answers.Count * 100, 1) 
                : 0,
            QuestionResults = answers.Select(a => new QuestionResult
            {
                QuestionId = a.QuestionId,
                IsCorrect = a.IsCorrect,
                SelectedOptionId = a.SelectedOptionId,
                TimeSpentSeconds = 60
            }).ToList(),
            CompletedAt = DateTime.UtcNow
        };
    }

    // ===== Statistics =====

    public async Task<StudyStatistics> GetStudyStatisticsAsync(long userId)
    {
        var answers = await _context.UserAnswerHistories
            .Where(h => h.UserId == userId)
            .ToListAsync();

        var sessions = await _context.StudySessions
            .Where(s => s.UserId == userId && s.Status == "completed")
            .ToListAsync();

        var today = DateTime.UtcNow.Date;
        var todayAnswers = answers.Where(a => a.ChangedAt?.Date == today).ToList();

        var streak = CalculateStreak(userId);

        var weakTopicIds = answers
            .GroupBy(a => a.QuestionId)
            .Select(g => new
            {
                QuestionId = g.Key,
                Attempts = g.Count(),
                Correct = g.Count(x => x.IsCorrect)
            })
            .Where(x => x.Attempts >= 3 && (decimal)x.Correct / x.Attempts < 0.5m)
            .Select(x => x.QuestionId)
            .ToList();

        var weakTopics = new List<WeakTopic>();
        foreach (var qId in weakTopicIds.Take(5))
        {
            var question = await _context.Questions.FindAsync(qId);
            if (question?.TopicId != null)
            {
                var topicAnswers = answers.Where(a => 
                    _context.Questions.Any(q => q.Id == a.QuestionId && q.TopicId == question.TopicId));
                
                weakTopics.Add(new WeakTopic
                {
                    TopicId = question.TopicId.Value,
                    TopicName = question.Topic?.Name ?? "Unknown",
                    TotalAttempts = topicAnswers.Count(),
                    Accuracy = topicAnswers.Count() > 0 
                        ? Math.Round((decimal)topicAnswers.Count(a => a.IsCorrect) / topicAnswers.Count() * 100, 1) 
                        : 0
                });
            }
        }

        return new StudyStatistics
        {
            TotalQuestionsAnswered = answers.Count,
            TotalCorrectAnswers = answers.Count(a => a.IsCorrect),
            OverallAccuracy = answers.Count > 0 
                ? Math.Round((decimal)answers.Count(a => a.IsCorrect) / answers.Count * 100, 1) 
                : 0,
            TotalStudyTimeMinutes = sessions.Sum(s => s.TimeSpentMinutes),
            CurrentStreak = streak,
            LongestStreak = streak,
            TotalStudyDays = sessions.Select(s => s.StartedAt.Date).Distinct().Count(),
            AverageSessionMinutes = sessions.Count > 0 ? sessions.Average(s => s.TimeSpentMinutes) : 0,
            QuestionsToday = todayAnswers.Count,
            CorrectToday = todayAnswers.Count(a => a.IsCorrect),
            WeakTopics = weakTopics,
            StrongTopics = new List<StrongTopic>()
        };
    }

    public async Task<List<TopicMastery>> GetTopicMasteryAsync(long userId)
    {
        var topics = await _context.Topics.ToListAsync();
        var result = new List<TopicMastery>();

        foreach (var topic in topics)
        {
            var topicQuestions = await _context.Questions
                .Where(q => q.TopicId == topic.Id)
                .ToListAsync();

            var topicQuestionIds = topicQuestions.Select(q => q.Id).ToList();

            var answers = await _context.UserAnswerHistories
                .Where(h => h.UserId == userId && topicQuestionIds.Contains(h.QuestionId))
                .ToListAsync();

            var total = answers.Count;
            var correct = answers.Count(a => a.IsCorrect);
            var mastery = total > 0 ? (decimal)correct / total * 100 : 0;

            result.Add(new TopicMastery
            {
                TopicId = topic.Id,
                TopicName = topic.Name,
                TotalQuestions = topicQuestions.Count,
                CorrectAnswers = correct,
                MasteryPercentage = Math.Round(mastery, 1),
                MasteryLevel = mastery switch
                {
                    >= 90 => "Master",
                    >= 70 => "Proficient",
                    >= 50 => "Intermediate",
                    >= 30 => "Beginner",
                    _ => "Novice"
                }
            });
        }

        return result.OrderByDescending(t => t.MasteryPercentage).ToList();
    }

    public async Task<SubjectProgress> GetSubjectProgressAsync(long userId, int subjectId)
    {
        var subject = await _context.Subjects.FindAsync(subjectId);
        if (subject == null)
            throw new InvalidOperationException("Subject not found");

        var allQuestions = await _context.Questions
            .Where(q => q.SubjectId == subjectId)
            .ToListAsync();

        var attemptedQuestionIds = await _context.UserAnswerHistories
            .Where(h => h.UserId == userId && allQuestions.Select(q => q.Id).Contains(h.QuestionId))
            .Select(h => h.QuestionId)
            .Distinct()
            .ToListAsync();

        var topicIds = allQuestions.Where(q => q.TopicId.HasValue).Select(q => q.TopicId!.Value).Distinct();
        var topicProgresses = new List<TopicProgress>();

        foreach (var topicId in topicIds)
        {
            var topic = await _context.Topics.FindAsync(topicId);
            var topicQuestions = allQuestions.Where(q => q.TopicId == topicId).ToList();
            var topicAttempted = attemptedQuestionIds.Count(id => topicQuestions.Any(q => q.Id == id));

            topicProgresses.Add(new TopicProgress
            {
                TopicId = topicId,
                TopicName = topic?.Name ?? "Unknown",
                TotalQuestions = topicQuestions.Count,
                Attempted = topicAttempted,
                Mastered = 0,
                MasteryPercentage = topicAttempted > 0 ? Math.Round((decimal)topicAttempted / topicQuestions.Count * 100, 1) : 0
            });
        }

        return new SubjectProgress
        {
            SubjectId = subjectId,
            SubjectName = subject.Name,
            TotalQuestions = allQuestions.Count,
            AttemptedQuestions = attemptedQuestionIds.Count,
            MasteredQuestions = 0,
            CompletionPercentage = allQuestions.Count > 0 
                ? Math.Round((decimal)attemptedQuestionIds.Count / allQuestions.Count * 100, 1) 
                : 0,
            TopicProgresses = topicProgresses
        };
    }

    private async Task<int> CalculateStreak(long userId)
    {
        var today = DateTime.UtcNow.Date;
        var streak = 0;

        for (int i = 0; i <= 365; i++)
        {
            var date = today.AddDays(-i);
            var hasActivity = await _context.UserDailyProgresses
                .AnyAsync(p => p.UserId == userId && p.Date == date && p.QuestionsAnswered > 0);

            if (hasActivity)
                streak++;
            else if (i > 0)
                break;
        }

        return streak;
    }
}

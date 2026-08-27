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

public class ReviewService : IReviewService
{
    private readonly BookstoreDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(BookstoreDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    // ===== Spaced Repetition System =====

    public async Task<ReviewCard> CreateReviewCardAsync(long userId, long questionId)
    {
        var existingCard = await _context.ReviewCards
            .FirstOrDefaultAsync(c => c.UserId == userId && c.QuestionId == questionId);

        if (existingCard != null)
            return MapToReviewCard(existingCard);

        var card = new ReviewCard
        {
            UserId = userId,
            QuestionId = questionId,
            NextReviewDate = DateTime.UtcNow,
            RepetitionCount = 0,
            EaseFactor = 2.5m,
            IntervalDays = 1,
            LastReviewDate = DateTime.UtcNow,
            IsMastered = false
        };

        _context.ReviewCards.Add(card);
        await _context.SaveChangesAsync();

        return MapToReviewCard(card);
    }

    public async Task<ReviewCard?> GetReviewCardAsync(long userId, long questionId)
    {
        var card = await _context.ReviewCards
            .FirstOrDefaultAsync(c => c.UserId == userId && c.QuestionId == questionId);

        return card == null ? null : MapToReviewCard(card);
    }

    public async Task<ReviewCard> UpdateReviewAsync(long userId, long questionId, ReviewRating rating)
    {
        var card = await _context.ReviewCards
            .FirstOrDefaultAsync(c => c.UserId == userId && c.QuestionId == questionId);

        if (card == null)
            card = await CreateReviewCardEntityAsync(userId, questionId);

        card = CalculateNextReview(card, rating);
        await _context.SaveChangesAsync();

        return MapToReviewCard(card);
    }

    private async Task<ReviewCard> CreateReviewCardEntityAsync(long userId, long questionId)
    {
        var card = new ReviewCard
        {
            UserId = userId,
            QuestionId = questionId,
            NextReviewDate = DateTime.UtcNow,
            RepetitionCount = 0,
            EaseFactor = 2.5m,
            IntervalDays = 1,
            LastReviewDate = DateTime.UtcNow,
            IsMastered = false
        };

        _context.ReviewCards.Add(card);
        return card;
    }

    private ReviewCard CalculateNextReview(ReviewCard card, ReviewRating rating)
    {
        card.LastReviewDate = DateTime.UtcNow;
        card.RepetitionCount++;

        card.EaseFactor = rating switch
        {
            ReviewRating.Again => Math.Max(1.3m, card.EaseFactor - 0.2m),
            ReviewRating.Hard => Math.Max(1.3m, card.EaseFactor - 0.15m),
            ReviewRating.Good => card.EaseFactor,
            ReviewRating.Easy => Math.Min(2.5m, card.EaseFactor + 0.15m),
            _ => card.EaseFactor
        };

        card.IntervalDays = rating switch
        {
            ReviewRating.Again => 1,
            ReviewRating.Hard => Math.Max(1, (int)(card.IntervalDays * 1.2m * (card.EaseFactor / 2.5m))),
            ReviewRating.Good => (int)(card.IntervalDays * card.EaseFactor),
            ReviewRating.Easy => (int)(card.IntervalDays * card.EaseFactor * 1.3m),
            _ => card.IntervalDays
        };

        card.NextReviewDate = DateTime.UtcNow.AddDays(card.IntervalDays);

        if (card.RepetitionCount >= 5 && card.EaseFactor >= 2.0m)
            card.IsMastered = true;

        return card;
    }

    public async Task<List<ReviewCard>> GetDueReviewCardsAsync(long userId, int limit = 20)
    {
        var now = DateTime.UtcNow;

        var cards = await _context.ReviewCards
            .Where(c => c.UserId == userId && c.NextReviewDate <= now && !c.IsMastered)
            .OrderBy(c => c.NextReviewDate)
            .Take(limit)
            .ToListAsync();

        return cards.Select(MapToReviewCard).ToList();
    }

    public async Task<int> GetDueReviewCountAsync(long userId)
    {
        var now = DateTime.UtcNow;

        return await _context.ReviewCards
            .CountAsync(c => c.UserId == userId && c.NextReviewDate <= now && !c.IsMastered);
    }

    // ===== Review Session =====

    public async Task<ReviewSession> StartReviewSessionAsync(long userId)
    {
        var session = new ReviewSession
        {
            UserId = userId,
            StartedAt = DateTime.UtcNow,
            Status = "active"
        };

        return session;
    }

    public async Task<ReviewSession?> GetReviewSessionAsync(long sessionId)
    {
        var session = await _context.ReviewSessions.FindAsync(sessionId);
        if (session == null) return null;

        return new ReviewSession
        {
            Id = session.Id,
            UserId = session.UserId,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt,
            CardsReviewed = session.CardsReviewed,
            CorrectCount = session.CorrectCount,
            TotalTimeSeconds = session.TotalTimeSeconds,
            Status = session.Status
        };
    }

    public async Task<ReviewSession> CompleteReviewSessionAsync(long sessionId)
    {
        var session = await _context.ReviewSessions.FindAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException("Review session not found");

        session.CompletedAt = DateTime.UtcNow;
        session.Status = "completed";
        await _context.SaveChangesAsync();

        return new ReviewSession
        {
            Id = session.Id,
            UserId = session.UserId,
            StartedAt = session.StartedAt,
            CompletedAt = session.CompletedAt,
            CardsReviewed = session.CardsReviewed,
            CorrectCount = session.CorrectCount,
            TotalTimeSeconds = session.TotalTimeSeconds,
            Status = session.Status
        };
    }

    // ===== Learning Analytics =====

    public async Task<ReviewAnalytics> GetReviewAnalyticsAsync(long userId)
    {
        var allCards = await _context.ReviewCards
            .Where(c => c.UserId == userId)
            .ToListAsync();

        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var dailyStats = await _context.UserDailyProgresses
            .Where(p => p.UserId == userId && p.Date >= weekAgo)
            .OrderBy(p => p.Date)
            .ToListAsync();

        var weakQuestionIds = allCards
            .Where(c => c.RepetitionCount >= 2 && c.EaseFactor < 2.0m)
            .OrderBy(c => c.EaseFactor)
            .Take(10)
            .Select(c => c.QuestionId)
            .ToList();

        var weakQuestions = new List<WeakQuestionInfo>();
        foreach (var qId in weakQuestionIds)
        {
            var question = await _context.Questions.FindAsync(qId);
            var answers = await _context.UserAnswerHistories
                .Where(h => h.UserId == userId && h.QuestionId == qId)
                .ToListAsync();

            weakQuestions.Add(new WeakQuestionInfo
            {
                QuestionId = qId,
                Content = question?.Content ?? "Unknown",
                AttemptCount = answers.Count,
                CorrectCount = answers.Count(a => a.IsCorrect),
                SuccessRate = answers.Count > 0 
                    ? Math.Round((decimal)answers.Count(a => a.IsCorrect) / answers.Count * 100, 1) 
                    : 0,
                LastAttempted = answers.OrderByDescending(a => a.ChangedAt).FirstOrDefault()?.ChangedAt ?? DateTime.MinValue
            });
        }

        return new ReviewAnalytics
        {
            TotalReviews = allCards.Sum(c => c.RepetitionCount),
            CardsLearned = allCards.Count(c => c.RepetitionCount > 0),
            CardsMastered = allCards.Count(c => c.IsMastered),
            CardsDueToday = await GetDueReviewCountAsync(userId),
            AverageEaseFactor = allCards.Count > 0 ? Math.Round(allCards.Average(c => c.EaseFactor), 2) : 2.5m,
            StudyDaysStreak = CalculateStudyStreak(userId),
            DailyStats = dailyStats.Select(d => new DailyReviewStats
            {
                Date = d.Date,
                CardsReviewed = d.QuestionsAnswered,
                CorrectCount = d.CorrectAnswers,
                RetentionRate = d.QuestionsAnswered > 0 
                    ? Math.Round((decimal)d.CorrectAnswers / d.QuestionsAnswered * 100, 1) 
                    : 0
            }).ToList(),
            WeakQuestions = weakQuestions
        };
    }

    public async Task<List<ReviewHistory>> GetReviewHistoryAsync(long userId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.UserAnswerHistories
            .Where(h => h.UserId == userId);

        if (fromDate.HasValue)
            query = query.Where(h => h.ChangedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(h => h.ChangedAt <= toDate.Value);

        var answers = await query
            .OrderByDescending(h => h.ChangedAt)
            .Take(100)
            .ToListAsync();

        return answers.Select(a => new ReviewHistory
        {
            QuestionId = a.QuestionId,
            ReviewedAt = a.ChangedAt ?? DateTime.UtcNow,
            Rating = a.IsCorrect ? ReviewRating.Good : ReviewRating.Again,
            TimeSpentSeconds = 60,
            WasCorrect = a.IsCorrect
        }).ToList();
    }

    private int CalculateStudyStreak(long userId)
    {
        var today = DateTime.UtcNow.Date;
        var streak = 0;

        for (int i = 0; i <= 365; i++)
        {
            var date = today.AddDays(-i);
            var hasActivity = _context.UserDailyProgresses
                .Any(p => p.UserId == userId && p.Date == date && p.QuestionsAnswered > 0);

            if (hasActivity)
                streak++;
            else if (i > 0)
                break;
        }

        return streak;
    }

    private static ReviewCard MapToReviewCard(ReviewCard card)
    {
        return new ReviewCard
        {
            UserId = card.UserId,
            QuestionId = card.QuestionId,
            NextReviewDate = card.NextReviewDate,
            RepetitionCount = card.RepetitionCount,
            EaseFactor = card.EaseFactor,
            IntervalDays = card.IntervalDays,
            LastReviewDate = card.LastReviewDate,
            IsMastered = card.IsMastered
        };
    }
}

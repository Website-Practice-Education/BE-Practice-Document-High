using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface IReviewService
{
    // ===== Spaced Repetition System =====
    Task<ReviewCard> CreateReviewCardAsync(long userId, long questionId);
    Task<ReviewCard?> GetReviewCardAsync(long userId, long questionId);
    Task<ReviewCard> UpdateReviewAsync(long userId, long questionId, ReviewRating rating);
    Task<List<ReviewCard>> GetDueReviewCardsAsync(long userId, int limit = 20);
    Task<int> GetDueReviewCountAsync(long userId);
    
    // ===== Review Session =====
    Task<ReviewSession> StartReviewSessionAsync(long userId);
    Task<ReviewSession?> GetReviewSessionAsync(long sessionId);
    Task<ReviewSession> CompleteReviewSessionAsync(long sessionId);
    
    // ===== Learning Analytics =====
    Task<ReviewAnalytics> GetReviewAnalyticsAsync(long userId);
    Task<List<ReviewHistory>> GetReviewHistoryAsync(long userId, DateTime? fromDate, DateTime? toDate);
}

public enum ReviewRating
{
    Again = 0,      // Complete blackout, wrong answer
    Hard = 1,       // Correct but with difficulty
    Good = 2,       // Correct with some hesitation
    Easy = 3        // Perfect, instant recall
}

public class ReviewCard
{
    public long UserId { get; set; }
    public long QuestionId { get; set; }
    public DateTime NextReviewDate { get; set; }
    public int RepetitionCount { get; set; }
    public decimal EaseFactor { get; set; }
    public int IntervalDays { get; set; }
    public DateTime LastReviewDate { get; set; }
    public bool IsMastered { get; set; }
}

public class ReviewSession
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int CardsReviewed { get; set; }
    public int CorrectCount { get; set; }
    public int TotalTimeSeconds { get; set; }
    public string Status { get; set; } = "active";
}

public class ReviewAnalytics
{
    public int TotalReviews { get; set; }
    public int CardsLearned { get; set; }
    public int CardsMastered { get; set; }
    public int CardsDueToday { get; set; }
    public decimal AverageEaseFactor { get; set; }
    public int StudyDaysStreak { get; set; }
    public List<DailyReviewStats> DailyStats { get; set; } = new();
    public List<WeakQuestionInfo> WeakQuestions { get; set; } = new();
}

public class DailyReviewStats
{
    public DateTime Date { get; set; }
    public int CardsReviewed { get; set; }
    public int CorrectCount { get; set; }
    public decimal RetentionRate { get; set; }
}

public class WeakQuestionInfo
{
    public long QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public int CorrectCount { get; set; }
    public decimal SuccessRate { get; set; }
    public DateTime LastAttempted { get; set; }
}

public class ReviewHistory
{
    public long QuestionId { get; set; }
    public DateTime ReviewedAt { get; set; }
    public ReviewRating Rating { get; set; }
    public int TimeSpentSeconds { get; set; }
    public bool WasCorrect { get; set; }
}

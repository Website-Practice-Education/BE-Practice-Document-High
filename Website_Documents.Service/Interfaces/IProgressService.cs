using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface IProgressService
{
    // Daily Progress
    Task<UserDailyProgress> UpdateDailyProgressAsync(long userId);
    Task<UserDailyProgress?> GetTodayProgressAsync(long userId);
    Task<List<UserDailyProgress>> GetWeeklyProgressAsync(long userId);
    Task<List<UserDailyProgress>> GetMonthlyProgressAsync(long userId, int year, int month);
    
    // Topic & Lesson Progress
    Task UpdateLessonProgressAsync(long userId, int lessonId, string status);
    Task<List<TopicProgressDto>> GetTopicProgressAsync(long userId);
    Task UpdateTopicProgressAsync(long userId, int topicId, string status);
    
    // Dashboard & Stats
    Task<DashboardResponse> GetDashboardAsync(long userId);
    Task<MonthlyStatsDto> GetMonthlyStatsAsync(long userId);
    Task<StreakDto> GetStreakAsync(long userId);
    
    // XP Transaction
    Task AddXpTransactionAsync(long userId, int amount, string reason, string? sourceType = null, long? sourceId = null);
}

public class DashboardResponse
{
    public int TotalQuestionsAnswered { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public decimal AccuracyRate { get; set; }
    public int TotalExamsTaken { get; set; }
    public decimal AverageScore { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int TotalStudyTimeMinutes { get; set; }
    public int LessonsCompleted { get; set; }
    public int TopicsCompleted { get; set; }
    public List<DailyProgressDto> WeeklyProgress { get; set; } = new();
}

public class DailyProgressDto
{
    public DateTime Date { get; set; }
    public int QuestionsAnswered { get; set; }
    public int CorrectAnswers { get; set; }
    public int TimeSpentMinutes { get; set; }
}

public class TopicProgressDto
{
    public int TopicId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public decimal Accuracy { get; set; }
    public DateTime? LastPracticedAt { get; set; }
}

public class MonthlyStatsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int CorrectQuestions { get; set; }
    public int TotalMinutes { get; set; }
    public int TotalExams { get; set; }
    public int TotalXP { get; set; }
    public int DaysActive { get; set; }
    public decimal AverageDailyXP { get; set; }
}

public class StreakDto
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastActivityDate { get; set; }
}

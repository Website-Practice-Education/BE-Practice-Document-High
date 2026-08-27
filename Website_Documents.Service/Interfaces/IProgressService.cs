using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface IProgressService
{
    Task<UserDailyProgress> UpdateDailyProgressAsync(long userId);
    Task<UserDailyProgress?> GetTodayProgressAsync(long userId);
    Task<List<UserDailyProgress>> GetWeeklyProgressAsync(long userId);
    Task UpdateLessonProgressAsync(long userId, int lessonId, string status);
    Task UpdateTopicProgressAsync(long userId, int topicId, string status);
    Task<DashboardResponse> GetDashboardAsync(long userId);
}

public class DashboardResponse
{
    public int TotalQuestionsAnswered { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public decimal AccuracyRate { get; set; }
    public int TotalExamsTaken { get; set; }
    public decimal AverageScore { get; set; }
    public int CurrentStreak { get; set; }
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface ILearningPlanService
{
    // ===== Learning Plan =====
    Task<LearningPlan> CreateLearningPlanAsync(long userId, CreateLearningPlanRequest request);
    Task<LearningPlan?> GetLearningPlanAsync(long planId);
    Task<List<LearningPlan>> GetUserLearningPlansAsync(long userId, bool? isActive);
    Task<LearningPlan> UpdateLearningPlanAsync(long planId, UpdateLearningPlanRequest request);
    Task<bool> DeleteLearningPlanAsync(long planId);
    
    // ===== Daily Study Goal =====
    Task<DailyGoal> SetDailyGoalAsync(long userId, int targetQuestions, int targetMinutes);
    Task<DailyGoal?> GetDailyGoalAsync(long userId);
    Task<DailyGoalProgress> GetDailyGoalProgressAsync(long userId);
    
    // ===== Study Reminder =====
    Task<StudyReminder> CreateReminderAsync(long userId, StudyReminderRequest request);
    Task<List<StudyReminder>> GetUserRemindersAsync(long userId);
    Task<bool> ToggleReminderAsync(long reminderId, bool isEnabled);
    Task<bool> DeleteReminderAsync(long reminderId);
    
    // ===== Study Streak =====
    Task<StudyStreak> GetStudyStreakAsync(long userId);
    Task<StudyStreak> UpdateStreakAsync(long userId);
    
    // ===== Study Recommendation =====
    Task<StudyRecommendation> GetRecommendationAsync(long userId);
    Task<List<StudyPlanItem>> GetTodayStudyPlanAsync(long userId);
}

public class LearningPlan
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TargetDays { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int DailyTargetQuestions { get; set; }
    public int DailyTargetMinutes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<LearningPlanItem> Items { get; set; } = new();
}

public class LearningPlanItem
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int? TopicId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int TargetQuestions { get; set; }
    public int CompletedQuestions { get; set; }
    public bool IsCompleted { get; set; }
}

public class CreateLearningPlanRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TargetDays { get; set; }
    public DateTime StartDate { get; set; }
    public int DailyTargetQuestions { get; set; }
    public int DailyTargetMinutes { get; set; }
    public List<PlanItemRequest> Items { get; set; } = new();
}

public class PlanItemRequest
{
    public int SubjectId { get; set; }
    public int? TopicId { get; set; }
    public int Priority { get; set; }
    public int TargetQuestions { get; set; }
}

public class UpdateLearningPlanRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? EndDate { get; set; }
    public int? DailyTargetQuestions { get; set; }
    public int? DailyTargetMinutes { get; set; }
    public bool? IsActive { get; set; }
}

public class DailyGoal
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public int TargetQuestions { get; set; }
    public int TargetMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DailyGoalProgress
{
    public int TargetQuestions { get; set; }
    public int CompletedQuestions { get; set; }
    public int TargetMinutes { get; set; }
    public int CompletedMinutes { get; set; }
    public decimal QuestionsProgress { get; set; }
    public decimal TimeProgress { get; set; }
    public bool IsQuestionsCompleted { get; set; }
    public bool IsTimeCompleted { get; set; }
    public List<GoalMilestone> Milestones { get; set; } = new();
}

public class GoalMilestone
{
    public string Label { get; set; } = string.Empty;
    public int Target { get; set; }
    public int Current { get; set; }
    public bool IsAchieved { get; set; }
}

public class StudyReminder
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public TimeSpan ReminderTime { get; set; }
    public List<DayOfWeek> DaysOfWeek { get; set; } = new();
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StudyReminderRequest
{
    public string Title { get; set; } = string.Empty;
    public TimeSpan ReminderTime { get; set; }
    public List<DayOfWeek> DaysOfWeek { get; set; } = new();
}

public class StudyStreak
{
    public long UserId { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastStudyDate { get; set; }
    public int TotalStudyDays { get; set; }
}

public class StudyRecommendation
{
    public string RecommendedAction { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int SuggestedQuestionCount { get; set; }
    public List<RecommendedTopic> RecommendedTopics { get; set; } = new();
    public List<RecommendedQuestion> RecommendedQuestions { get; set; } = new();
}

public class RecommendedTopic
{
    public int TopicId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int SuggestedQuestions { get; set; }
}

public class RecommendedQuestion
{
    public long QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class StudyPlanItem
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int? TopicId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public int SuggestedQuestions { get; set; }
    public int CompletedQuestions { get; set; }
    public string Priority { get; set; } = "Medium";
}

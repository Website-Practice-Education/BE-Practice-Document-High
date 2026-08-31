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

// Type aliases to resolve ambiguity between Repository.Models and Service.Interfaces
using RepoModels = Website_Documents.Repository.Models;
using ServiceModels = Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class LearningPlanService : ILearningPlanService
{
    private readonly BookstoreDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public LearningPlanService(BookstoreDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    // ===== Learning Plan =====

    public async Task<ServiceModels.LearningPlan> CreateLearningPlanAsync(long userId, ServiceModels.CreateLearningPlanRequest request)
    {
        var plan = new RepoModels.LearningPlan
        {
            UserId = userId,
            Title = request.Title,
            Description = request.Description,
            TargetDays = request.TargetDays,
            StartDate = request.StartDate,
            EndDate = request.StartDate.AddDays(request.TargetDays),
            DailyTargetQuestions = request.DailyTargetQuestions,
            DailyTargetMinutes = request.DailyTargetMinutes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.LearningPlans.Add(plan);
        await _context.SaveChangesAsync();

        return new ServiceModels.LearningPlan
        {
            Id = plan.Id,
            UserId = plan.UserId,
            Title = plan.Title,
            Description = plan.Description ?? string.Empty,
            TargetDays = plan.TargetDays,
            StartDate = plan.StartDate,
            EndDate = plan.EndDate,
            DailyTargetQuestions = plan.DailyTargetQuestions,
            DailyTargetMinutes = plan.DailyTargetMinutes,
            IsActive = plan.IsActive,
            CreatedAt = plan.CreatedAt,
            Items = new List<ServiceModels.LearningPlanItem>()
        };
    }

    public async Task<ServiceModels.LearningPlan?> GetLearningPlanAsync(long planId)
    {
        var plan = await _context.LearningPlans
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null) return null;

        return new ServiceModels.LearningPlan
        {
            Id = plan.Id,
            UserId = plan.UserId,
            Title = plan.Title,
            Description = plan.Description ?? string.Empty,
            TargetDays = plan.TargetDays,
            StartDate = plan.StartDate,
            EndDate = plan.EndDate,
            DailyTargetQuestions = plan.DailyTargetQuestions,
            DailyTargetMinutes = plan.DailyTargetMinutes,
            IsActive = plan.IsActive,
            CreatedAt = plan.CreatedAt,
            Items = plan.Items.Select(i => new ServiceModels.LearningPlanItem
            {
                SubjectId = i.SubjectId,
                SubjectName = i.Subject?.Name ?? string.Empty,
                TopicId = i.TopicId,
                TopicName = i.Topic?.Name ?? string.Empty,
                Priority = i.Priority,
                TargetQuestions = i.TargetQuestions,
                CompletedQuestions = i.CompletedQuestions,
                IsCompleted = i.IsCompleted
            }).ToList()
        };
    }

    public async Task<List<ServiceModels.LearningPlan>> GetUserLearningPlansAsync(long userId, bool? isActive)
    {
        var query = _context.LearningPlans.Where(p => p.UserId == userId);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var plans = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

        return plans.Select(plan => new ServiceModels.LearningPlan
        {
            Id = plan.Id,
            UserId = plan.UserId,
            Title = plan.Title,
            Description = plan.Description ?? string.Empty,
            TargetDays = plan.TargetDays,
            StartDate = plan.StartDate,
            EndDate = plan.EndDate,
            DailyTargetQuestions = plan.DailyTargetQuestions,
            DailyTargetMinutes = plan.DailyTargetMinutes,
            IsActive = plan.IsActive,
            CreatedAt = plan.CreatedAt,
            Items = new List<ServiceModels.LearningPlanItem>()
        }).ToList();
    }

    public async Task<ServiceModels.LearningPlan> UpdateLearningPlanAsync(long planId, ServiceModels.UpdateLearningPlanRequest request)
    {
        var plan = await _context.LearningPlans.FindAsync(planId);
        if (plan == null)
            throw new InvalidOperationException("Learning plan not found");

        if (!string.IsNullOrEmpty(request.Title))
            plan.Title = request.Title;

        if (!string.IsNullOrEmpty(request.Description))
            plan.Description = request.Description;

        if (request.EndDate.HasValue)
            plan.EndDate = request.EndDate;

        if (request.DailyTargetQuestions.HasValue)
            plan.DailyTargetQuestions = request.DailyTargetQuestions.Value;

        if (request.DailyTargetMinutes.HasValue)
            plan.DailyTargetMinutes = request.DailyTargetMinutes.Value;

        if (request.IsActive.HasValue)
            plan.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync();

        return new ServiceModels.LearningPlan
        {
            Id = plan.Id,
            UserId = plan.UserId,
            Title = plan.Title,
            Description = plan.Description ?? string.Empty,
            TargetDays = plan.TargetDays,
            StartDate = plan.StartDate,
            EndDate = plan.EndDate,
            DailyTargetQuestions = plan.DailyTargetQuestions,
            DailyTargetMinutes = plan.DailyTargetMinutes,
            IsActive = plan.IsActive,
            CreatedAt = plan.CreatedAt,
            Items = new List<ServiceModels.LearningPlanItem>()
        };
    }

    public async Task<bool> DeleteLearningPlanAsync(long planId)
    {
        var plan = await _context.LearningPlans.FindAsync(planId);
        if (plan == null) return false;

        plan.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Daily Goal =====

    public async Task<ServiceModels.DailyGoal> SetDailyGoalAsync(long userId, int targetQuestions, int targetMinutes)
    {
        var today = DateTime.UtcNow.Date;
        var existingGoal = await _context.DailyGoals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.Date.Date == today);

        if (existingGoal != null)
        {
            existingGoal.TargetQuestions = targetQuestions;
            existingGoal.TargetMinutes = targetMinutes;
            await _context.SaveChangesAsync();

            return new ServiceModels.DailyGoal
            {
                Id = existingGoal.Id,
                UserId = existingGoal.UserId,
                TargetQuestions = existingGoal.TargetQuestions,
                TargetMinutes = existingGoal.TargetMinutes,
                CreatedAt = existingGoal.CreatedAt
            };
        }

        var goal = new RepoModels.DailyGoal
        {
            UserId = userId,
            TargetQuestions = targetQuestions,
            TargetMinutes = targetMinutes,
            Date = today,
            CreatedAt = DateTime.UtcNow
        };

        _context.DailyGoals.Add(goal);
        await _context.SaveChangesAsync();

        return new ServiceModels.DailyGoal
        {
            Id = goal.Id,
            UserId = goal.UserId,
            TargetQuestions = goal.TargetQuestions,
            TargetMinutes = goal.TargetMinutes,
            CreatedAt = goal.CreatedAt
        };
    }

    public async Task<ServiceModels.DailyGoal?> GetDailyGoalAsync(long userId)
    {
        var today = DateTime.UtcNow.Date;
        var goal = await _context.DailyGoals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.Date.Date == today);

        if (goal == null) return null;

        return new ServiceModels.DailyGoal
        {
            Id = goal.Id,
            UserId = goal.UserId,
            TargetQuestions = goal.TargetQuestions,
            TargetMinutes = goal.TargetMinutes,
            CreatedAt = goal.CreatedAt
        };
    }

    public async Task<ServiceModels.DailyGoalProgress> GetDailyGoalProgressAsync(long userId)
    {
        var today = DateTime.UtcNow.Date;

        var goal = await GetDailyGoalAsync(userId);
        var todayProgress = await _context.UserDailyProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Date == today);

        var completedQuestions = todayProgress?.QuestionsAnswered ?? 0;
        var completedMinutes = todayProgress?.TimeSpentMinutes ?? 0;

        var targetQuestions = goal?.TargetQuestions ?? 10;
        var targetMinutes = goal?.TargetMinutes ?? 30;

        var milestones = new List<ServiceModels.GoalMilestone>
        {
            new() { Label = "25%", Target = targetQuestions, Current = completedQuestions, IsAchieved = completedQuestions >= targetQuestions * 0.25m },
            new() { Label = "50%", Target = targetQuestions, Current = completedQuestions, IsAchieved = completedQuestions >= targetQuestions * 0.5m },
            new() { Label = "75%", Target = targetQuestions, Current = completedQuestions, IsAchieved = completedQuestions >= targetQuestions * 0.75m },
            new() { Label = "100%", Target = targetQuestions, Current = completedQuestions, IsAchieved = completedQuestions >= targetQuestions }
        };

        return new ServiceModels.DailyGoalProgress
        {
            TargetQuestions = targetQuestions,
            CompletedQuestions = completedQuestions,
            TargetMinutes = targetMinutes,
            CompletedMinutes = completedMinutes,
            QuestionsProgress = targetQuestions > 0 ? (decimal)completedQuestions / targetQuestions * 100 : 0,
            TimeProgress = targetMinutes > 0 ? (decimal)completedMinutes / targetMinutes * 100 : 0,
            IsQuestionsCompleted = completedQuestions >= targetQuestions,
            IsTimeCompleted = completedMinutes >= targetMinutes,
            Milestones = milestones
        };
    }

    // ===== Study Reminder =====

    public async Task<ServiceModels.StudyReminder> CreateReminderAsync(long userId, ServiceModels.StudyReminderRequest request)
    {
        var reminder = new RepoModels.StudyReminder
        {
            UserId = userId,
            Title = request.Title,
            ReminderTime = request.ReminderTime,
            DaysOfWeek = string.Join(",", request.DaysOfWeek.Select(d => (int)d)),
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.StudyReminders.Add(reminder);
        await _context.SaveChangesAsync();

        return new ServiceModels.StudyReminder
        {
            Id = reminder.Id,
            UserId = reminder.UserId,
            Title = reminder.Title,
            ReminderTime = reminder.ReminderTime,
            DaysOfWeek = reminder.DaysOfWeek?.Split(',').Select(int.Parse).Cast<DayOfWeek>().ToList() ?? new List<DayOfWeek>(),
            IsEnabled = reminder.IsEnabled,
            CreatedAt = reminder.CreatedAt
        };
    }

    public async Task<List<ServiceModels.StudyReminder>> GetUserRemindersAsync(long userId)
    {
        var reminders = await _context.StudyReminders
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.ReminderTime)
            .ToListAsync();

        return reminders.Select(r => new ServiceModels.StudyReminder
        {
            Id = r.Id,
            UserId = r.UserId,
            Title = r.Title,
            ReminderTime = r.ReminderTime,
            DaysOfWeek = r.DaysOfWeek?.Split(',').Select(int.Parse).Cast<DayOfWeek>().ToList() ?? new List<DayOfWeek>(),
            IsEnabled = r.IsEnabled,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<bool> ToggleReminderAsync(long reminderId, bool isEnabled)
    {
        var reminder = await _context.StudyReminders.FindAsync(reminderId);
        if (reminder == null) return false;

        reminder.IsEnabled = isEnabled;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteReminderAsync(long reminderId)
    {
        var reminder = await _context.StudyReminders.FindAsync(reminderId);
        if (reminder == null) return false;

        _context.StudyReminders.Remove(reminder);
        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Study Streak =====

    public async Task<ServiceModels.StudyStreak> GetStudyStreakAsync(long userId)
    {
        var today = DateTime.UtcNow.Date;
        var streak = 0;
        var longestStreak = 0;
        var lastStudyDate = (DateTime?)null;
        var totalStudyDays = 0;

        for (int i = 0; i <= 365; i++)
        {
            var date = today.AddDays(-i);
            var hasActivity = await _context.UserDailyProgresses
                .AnyAsync(p => p.UserId == userId && p.Date == date && p.QuestionsAnswered > 0);

            if (hasActivity)
            {
                if (i == 0 || (today.AddDays(-(i - 1)) == date.AddDays(1) && streak > 0))
                    streak++;
                else
                    streak = 1;

                longestStreak = Math.Max(longestStreak, streak);
                lastStudyDate = date;
                totalStudyDays++;
            }
            else if (i > 0)
            {
                break;
            }
        }

        return new ServiceModels.StudyStreak
        {
            UserId = userId,
            CurrentStreak = streak,
            LongestStreak = longestStreak,
            LastStudyDate = lastStudyDate,
            TotalStudyDays = totalStudyDays
        };
    }

    public async Task<ServiceModels.StudyStreak> UpdateStreakAsync(long userId)
    {
        return await GetStudyStreakAsync(userId);
    }

    // ===== Study Recommendation =====

    public async Task<ServiceModels.StudyRecommendation> GetRecommendationAsync(long userId)
    {
        var recommendation = new ServiceModels.StudyRecommendation();

        var weakTopics = await GetWeakTopicsForUserAsync(userId);
        if (weakTopics.Any())
        {
            recommendation.RecommendedAction = "Practice weak topics";
            recommendation.Reason = "Focus on improving your weaker areas";
            recommendation.SuggestedQuestionCount = 20;
            recommendation.RecommendedTopics = weakTopics.Take(3).ToList();
        }
        else
        {
            recommendation.RecommendedAction = "Continue regular practice";
            recommendation.Reason = "You're doing great! Keep up the good work";
            recommendation.SuggestedQuestionCount = 10;
        }

        return recommendation;
    }

    public async Task<List<ServiceModels.StudyPlanItem>> GetTodayStudyPlanAsync(long userId)
    {
        var planItems = new List<ServiceModels.StudyPlanItem>();
        var subjects = await _context.Subjects.ToListAsync();

        foreach (var subject in subjects.Take(5))
        {
            var questions = await _context.Questions
                .Where(q => q.SubjectId == subject.Id && q.IsActive == true)
                .ToListAsync();

            var attemptedIds = await _context.UserAnswerHistories
                .Where(h => h.UserId == userId && questions.Select(q => q.Id).Contains(h.QuestionId))
                .Select(h => h.QuestionId)
                .Distinct()
                .ToListAsync();

            var notAttempted = questions.Count(q => !attemptedIds.Contains(q.Id));

            planItems.Add(new ServiceModels.StudyPlanItem
            {
                SubjectId = subject.Id,
                SubjectName = subject.Name,
                TopicId = null,
                TopicName = "",
                SuggestedQuestions = Math.Min(5, notAttempted),
                CompletedQuestions = 0,
                Priority = "Medium"
            });
        }

        return planItems;
    }

    private async Task<List<ServiceModels.RecommendedTopic>> GetWeakTopicsForUserAsync(long userId)
    {
        var topics = await _context.Topics.ToListAsync();
        var weakTopics = new List<ServiceModels.RecommendedTopic>();

        foreach (var topic in topics)
        {
            var topicQuestions = await _context.Questions
                .Where(q => q.TopicId == topic.Id)
                .ToListAsync();

            var answers = await _context.UserAnswerHistories
                .Where(h => h.UserId == userId && topicQuestions.Select(q => q.Id).Contains(h.QuestionId))
                .ToListAsync();

            if (answers.Count >= 3)
            {
                var accuracy = (decimal)answers.Count(a => a.IsCorrect == true) / answers.Count * 100;
                if (accuracy < 70)
                {
                    weakTopics.Add(new ServiceModels.RecommendedTopic
                    {
                        TopicId = topic.Id,
                        TopicName = topic.Name,
                        Reason = $"{Math.Round(accuracy, 1)}% accuracy - needs more practice",
                        SuggestedQuestions = 10
                    });
                }
            }
        }

        return weakTopics.OrderBy(t => t.TopicName).ToList();
    }
}

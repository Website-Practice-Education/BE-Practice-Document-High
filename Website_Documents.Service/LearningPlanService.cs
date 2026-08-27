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

    public async Task<LearningPlan> CreateLearningPlanAsync(long userId, CreateLearningPlanRequest request)
    {
        var plan = new LearningPlan
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

        return plan;
    }

    public async Task<LearningPlan?> GetLearningPlanAsync(long planId)
    {
        return await _context.LearningPlans
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == planId);
    }

    public async Task<List<LearningPlan>> GetUserLearningPlansAsync(long userId, bool? isActive)
    {
        var query = _context.LearningPlans.Where(p => p.UserId == userId);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<LearningPlan> UpdateLearningPlanAsync(long planId, UpdateLearningPlanRequest request)
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
        return plan;
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

    public async Task<DailyGoal> SetDailyGoalAsync(long userId, int targetQuestions, int targetMinutes)
    {
        var today = DateTime.UtcNow.Date;
        var existingGoal = await _context.DailyGoals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.Date.Date == today);

        if (existingGoal != null)
        {
            existingGoal.TargetQuestions = targetQuestions;
            existingGoal.TargetMinutes = targetMinutes;
            await _context.SaveChangesAsync();
            return existingGoal;
        }

        var goal = new DailyGoal
        {
            UserId = userId,
            TargetQuestions = targetQuestions,
            TargetMinutes = targetMinutes,
            Date = today,
            CreatedAt = DateTime.UtcNow
        };

        _context.DailyGoals.Add(goal);
        await _context.SaveChangesAsync();
        return goal;
    }

    public async Task<DailyGoal?> GetDailyGoalAsync(long userId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.DailyGoals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.Date.Date == today);
    }

    public async Task<DailyGoalProgress> GetDailyGoalProgressAsync(long userId)
    {
        var today = DateTime.UtcNow.Date;

        var goal = await GetDailyGoalAsync(userId);
        var todayProgress = await _context.UserDailyProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Date == today);

        var completedQuestions = todayProgress?.QuestionsAnswered ?? 0;
        var completedMinutes = todayProgress?.TimeSpentMinutes ?? 0;

        var targetQuestions = goal?.TargetQuestions ?? 10;
        var targetMinutes = goal?.TargetMinutes ?? 30;

        var milestones = new List<GoalMilestone>
        {
            new() { Label = "25%", Target = targetQuestions, Current = completedQuestions, IsAchieved = completedQuestions >= targetQuestions * 0.25m },
            new() { Label = "50%", Target = targetQuestions, Current = completedQuestions, IsAchieved = completedQuestions >= targetQuestions * 0.5m },
            new() { Label = "75%", Target = targetQuestions, Current = completedQuestions, IsAchieved = completedQuestions >= targetQuestions * 0.75m },
            new() { Label = "100%", Target = targetQuestions, Current = completedQuestions, IsAchieved = completedQuestions >= targetQuestions }
        };

        return new DailyGoalProgress
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

    public async Task<StudyReminder> CreateReminderAsync(long userId, StudyReminderRequest request)
    {
        var reminder = new StudyReminder
        {
            UserId = userId,
            Title = request.Title,
            ReminderTime = request.ReminderTime,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        return reminder;
    }

    public async Task<List<StudyReminder>> GetUserRemindersAsync(long userId)
    {
        return await _context.StudyReminders
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.ReminderTime)
            .ToListAsync();
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

    public async Task<StudyStreak> GetStudyStreakAsync(long userId)
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

        return new StudyStreak
        {
            UserId = userId,
            CurrentStreak = streak,
            LongestStreak = longestStreak,
            LastStudyDate = lastStudyDate,
            TotalStudyDays = totalStudyDays
        };
    }

    public async Task<StudyStreak> UpdateStreakAsync(long userId)
    {
        return await GetStudyStreakAsync(userId);
    }

    // ===== Study Recommendation =====

    public async Task<StudyRecommendation> GetRecommendationAsync(long userId)
    {
        var recommendation = new StudyRecommendation();

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

    public async Task<List<StudyPlanItem>> GetTodayStudyPlanAsync(long userId)
    {
        var planItems = new List<StudyPlanItem>();
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

            planItems.Add(new StudyPlanItem
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

    private async Task<List<RecommendedTopic>> GetWeakTopicsForUserAsync(long userId)
    {
        var topics = await _context.Topics.ToListAsync();
        var weakTopics = new List<RecommendedTopic>();

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
                var accuracy = (decimal)answers.Count(a => a.IsCorrect) / answers.Count * 100;
                if (accuracy < 70)
                {
                    weakTopics.Add(new RecommendedTopic
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

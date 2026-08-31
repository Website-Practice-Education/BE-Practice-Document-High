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

public class ProgressService : IProgressService
{
    private readonly BookstoreDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public ProgressService(BookstoreDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDailyProgress> UpdateDailyProgressAsync(long userId)
    {
        var today = DateTime.UtcNow.Date;

        var progress = await _context.UserDailyProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Date == today);

        if (progress == null)
        {
            progress = new UserDailyProgress
            {
                UserId = userId,
                Date = today,
                QuestionsAnswered = 0,
                CorrectAnswers = 0,
                TimeSpentMinutes = 0
            };
            _context.UserDailyProgresses.Add(progress);
        }

        var todayAnswers = await _context.UserAnswerHistories
            .Where(h => h.UserId == userId && h.ChangedAt.HasValue && h.ChangedAt.Value.Date == today)
            .ToListAsync();

        progress.QuestionsAnswered = todayAnswers.Count;
        progress.CorrectAnswers = todayAnswers.Count(a => a.IsCorrect == true);

        await _context.SaveChangesAsync();
        return progress;
    }

    public async Task<UserDailyProgress?> GetTodayProgressAsync(long userId)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.UserDailyProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Date == today);
    }

    public async Task<List<UserDailyProgress>> GetWeeklyProgressAsync(long userId)
    {
        var weekAgo = DateTime.UtcNow.Date.AddDays(-7);
        return await _context.UserDailyProgresses
            .Where(p => p.UserId == userId && p.Date >= weekAgo)
            .OrderBy(p => p.Date)
            .ToListAsync();
    }

    public async Task UpdateLessonProgressAsync(long userId, int lessonId, string status)
    {
        var progress = await _context.UserLessonProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

        if (progress == null)
        {
            progress = new UserLessonProgress
            {
                UserId = userId,
                LessonId = lessonId,
                Status = status,
                CompletedAt = status == "completed" ? DateTime.UtcNow : null
            };
            _context.UserLessonProgresses.Add(progress);
        }
        else
        {
            progress.Status = status;
            progress.CompletedAt = status == "completed" ? DateTime.UtcNow : null;
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateTopicProgressAsync(long userId, int topicId, string status)
    {
        var progress = await _context.UserTopicProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.TopicId == topicId);

        if (progress == null)
        {
            progress = new UserTopicProgress
            {
                UserId = userId,
                TopicId = topicId,
                Status = status
            };
            _context.UserTopicProgresses.Add(progress);
        }
        else
        {
            progress.Status = status;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<DashboardResponse> GetDashboardAsync(long userId)
    {
        var totalAnswers = await _context.UserAnswerHistories
            .Where(h => h.UserId == userId)
            .ToListAsync();

        var totalExams = await _context.UserAttempts
            .Where(a => a.UserId == userId && a.Status == "submitted")
            .ToListAsync();

        var weeklyProgress = await GetWeeklyProgressAsync(userId);
        var lessonsCompleted = await _context.UserLessonProgresses
            .CountAsync(p => p.UserId == userId && p.Status == "completed");
        var topicsCompleted = await _context.UserTopicProgresses
            .CountAsync(p => p.UserId == userId && p.Status == "completed");

        var streak = CalculateStreak(weeklyProgress);

        return new DashboardResponse
        {
            TotalQuestionsAnswered = totalAnswers.Count,
            TotalCorrectAnswers = totalAnswers.Count(a => a.IsCorrect == true),
            AccuracyRate = totalAnswers.Count > 0
                ? Math.Round((decimal)totalAnswers.Count(a => a.IsCorrect == true) / totalAnswers.Count * 100, 1)
                : 0,
            TotalExamsTaken = totalExams.Count,
            AverageScore = totalExams.Count > 0
                ? Math.Round(totalExams.Average(a => a.Score ?? 0), 1)
                : 0,
            CurrentStreak = streak,
            TotalStudyTimeMinutes = totalAnswers.Count * 2,
            LessonsCompleted = lessonsCompleted,
            TopicsCompleted = topicsCompleted,
            WeeklyProgress = weeklyProgress.Select(p => new DailyProgressDto
            {
                Date = p.Date,
                QuestionsAnswered = p.QuestionsAnswered ?? 0,
                CorrectAnswers = p.CorrectAnswers ?? 0,
                TimeSpentMinutes = p.TimeSpentMinutes ?? 0
            }).ToList()
        };
    }

    private int CalculateStreak(List<UserDailyProgress> weeklyProgress)
    {
        if (!weeklyProgress.Any()) return 0;

        var streak = 0;
        var today = DateTime.UtcNow.Date;

        for (int i = 0; i <= 7; i++)
        {
            var date = today.AddDays(-i);
            var dayProgress = weeklyProgress.FirstOrDefault(p => p.Date == date);
            
            if (dayProgress != null && dayProgress.QuestionsAnswered > 0)
                streak++;
            else if (i > 0)
                break;
        }

        return streak;
    }
}

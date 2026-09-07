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

    // XP reward constants
    private const int XP_PER_CORRECT_ANSWER = 5;
    private const int XP_PER_EXAM_COMPLETE = 50;
    private const int XP_PER_LESSON_COMPLETE = 30;
    private const int XP_PER_TOPIC_COMPLETE = 100;
    private const int XP_STREAK_BONUS = 10; // Bonus per day of streak

    public ProgressService(BookstoreDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    #region Daily Progress

    public async Task<UserDailyProgress> UpdateDailyProgressAsync(long userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var progress = await _context.UserDailyProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.ProgressDate == today);

        if (progress == null)
        {
            progress = new UserDailyProgress
            {
                UserId = userId,
                ProgressDate = today,
                QuestionsAnswered = 0,
                QuestionsCorrect = 0,
                TimeSpentMinutes = 0,
                ExamsCompleted = 0,
                XpEarned = 0
            };
            _context.UserDailyProgresses.Add(progress);
        }

        // Calculate today's progress from user answers
        // Using user_answers which has is_correct column
        var todayAttempts = await _context.UserAttempts
            .Where(a => a.UserId == userId && a.Status == "completed" && a.SubmittedAt.HasValue && 
                        DateOnly.FromDateTime(a.SubmittedAt.Value) == today)
            .Select(a => a.Id)
            .ToListAsync();

        var todayAnswers = await _context.UserAnswers
            .Where(ua => todayAttempts.Contains(ua.AttemptId))
            .ToListAsync();

        // Calculate today's exams
        var todayExams = await _context.UserAttempts
            .Where(a => a.UserId == userId && a.Status == "completed" && a.SubmittedAt.HasValue && 
                        DateOnly.FromDateTime(a.SubmittedAt.Value) == today)
            .CountAsync();

        progress.QuestionsAnswered = todayAnswers.Count;
        progress.QuestionsCorrect = todayAnswers.Count(a => a.IsCorrect == true);
        // Estimate time spent: 2 minutes per question
        progress.TimeSpentMinutes = todayAnswers.Count * 2;
        progress.ExamsCompleted = todayExams;

        // Calculate XP earned today
        // XP = (correct answers * 5) + (completed exams * 50)
        var xpFromCorrectAnswers = (progress.QuestionsCorrect ?? 0) * XP_PER_CORRECT_ANSWER;
        var xpFromExams = todayExams * XP_PER_EXAM_COMPLETE;
        progress.XpEarned = xpFromCorrectAnswers + xpFromExams;

        await _context.SaveChangesAsync();

        // Update user XP
        var totalXpEarned = xpFromCorrectAnswers + xpFromExams;
        if (totalXpEarned > 0)
        {
            await AddXpTransactionAsync(userId, totalXpEarned, "Daily activity", "daily_progress");
        }

        return progress;
    }

    public async Task<UserDailyProgress?> GetTodayProgressAsync(long userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        return await _context.UserDailyProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.ProgressDate == today);
    }

    public async Task<List<UserDailyProgress>> GetWeeklyProgressAsync(long userId)
    {
        var weekAgo = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-7));
        return await _context.UserDailyProgresses
            .Where(p => p.UserId == userId && p.ProgressDate >= weekAgo)
            .OrderBy(p => p.ProgressDate)
            .ToListAsync();
    }

    public async Task<List<UserDailyProgress>> GetMonthlyProgressAsync(long userId, int year, int month)
    {
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        return await _context.UserDailyProgresses
            .Where(p => p.UserId == userId && p.ProgressDate >= startDate && p.ProgressDate <= endDate)
            .OrderBy(p => p.ProgressDate)
            .ToListAsync();
    }

    #endregion

    #region Topic & Lesson Progress

    public async Task<List<TopicProgressDto>> GetTopicProgressAsync(long userId)
    {
        // Get all topics with user progress
        var topicProgress = await _context.UserTopicProgresses
            .Include(p => p.Topic)
            .Where(p => p.UserId == userId)
            .ToListAsync();

        // Get all topics
        var allTopics = await _context.Topics
            .Include(t => t.Subject)
            .Where(t => t.IsActive == true)
            .ToListAsync();

        // Calculate progress for each topic
        var result = new List<TopicProgressDto>();
        foreach (var topic in allTopics)
        {
            var progress = topicProgress.FirstOrDefault(p => p.TopicId == topic.Id);
            
            // Get questions for this topic
            var topicQuestions = await _context.Questions
                .Where(q => q.TopicId == topic.Id)
                .CountAsync();

            // Get user answers for this topic
            var userAnswers = await _context.UserAnswers
                .Include(ua => ua.Attempt)
                .Where(ua => ua.Attempt.UserId == userId && ua.Question.TopicId == topic.Id)
                .ToListAsync();

            var totalQuestions = userAnswers.Count;
            var correctCount = userAnswers.Count(ua => ua.IsCorrect == true);
            var accuracy = totalQuestions > 0 ? Math.Round((decimal)correctCount / totalQuestions * 100, 1) : 0;

            result.Add(new TopicProgressDto
            {
                TopicId = topic.Id,
                TopicName = topic.Name,
                TotalQuestions = totalQuestions,
                CorrectCount = correctCount,
                Accuracy = accuracy,
                LastPracticedAt = progress?.LastPracticedAt
            });
        }

        return result.OrderByDescending(t => t.LastPracticedAt ?? DateTime.MinValue).ToList();
    }

    public async Task UpdateLessonProgressAsync(long userId, int lessonId, string status)
    {
        var progress = await _context.UserLessonProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

        var isNewlyCompleted = false;
        if (progress == null)
        {
            progress = new UserLessonProgress
            {
                UserId = userId,
                LessonId = lessonId,
                Status = status,
                StartedAt = DateTime.UtcNow,
                CompletedAt = status == "completed" ? DateTime.UtcNow : null,
                LastAccessedAt = DateTime.UtcNow,
                ProgressPercent = status == "completed" ? (short)100 : (short)0
            };
            _context.UserLessonProgresses.Add(progress);
            isNewlyCompleted = status == "completed";
        }
        else
        {
            var wasCompleted = progress.Status == "completed";
            progress.Status = status;
            progress.LastAccessedAt = DateTime.UtcNow;
            
            if (status == "completed" && !wasCompleted)
            {
                progress.CompletedAt = DateTime.UtcNow;
                progress.ProgressPercent = 100;
                isNewlyCompleted = true;
            }
        }

        await _context.SaveChangesAsync();

        // Award XP for completing a lesson
        if (isNewlyCompleted)
        {
            await AddXpTransactionAsync(userId, XP_PER_LESSON_COMPLETE, "Hoàn thành bài học", "lesson", lessonId);
            await UpdateDailyProgressAsync(userId);
        }
    }

    public async Task UpdateTopicProgressAsync(long userId, int topicId, string status)
    {
        var progress = await _context.UserTopicProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.TopicId == topicId);

        var isNewlyCompleted = false;
        if (progress == null)
        {
            progress = new UserTopicProgress
            {
                UserId = userId,
                TopicId = topicId,
                Status = status,
                LastPracticedAt = DateTime.UtcNow
            };
            _context.UserTopicProgresses.Add(progress);
            isNewlyCompleted = status == "completed";
        }
        else
        {
            var wasCompleted = progress.Status == "completed";
            progress.Status = status;
            progress.LastPracticedAt = DateTime.UtcNow;
            
            if (status == "completed" && !wasCompleted)
            {
                isNewlyCompleted = true;
            }
        }

        await _context.SaveChangesAsync();

        // Award XP for completing a topic
        if (isNewlyCompleted)
        {
            await AddXpTransactionAsync(userId, XP_PER_TOPIC_COMPLETE, "Hoàn thành chủ đề", "topic", topicId);
            await UpdateDailyProgressAsync(userId);
        }
    }

    #endregion

    #region Dashboard & Stats

    public async Task<DashboardResponse> GetDashboardAsync(long userId)
    {
        // Get all user attempts and their answers
        var allAttemptIds = await _context.UserAttempts
            .Where(a => a.UserId == userId)
            .Select(a => a.Id)
            .ToListAsync();

        var totalAnswers = await _context.UserAnswers
            .Where(ua => allAttemptIds.Contains(ua.AttemptId))
            .ToListAsync();

        // Get all completed exams
        var totalExams = await _context.UserAttempts
            .Where(a => a.UserId == userId && a.Status == "completed")
            .ToListAsync();

        // Get weekly progress
        var weeklyProgress = await GetWeeklyProgressAsync(userId);
        
        // Get completed lessons and topics
        var lessonsCompleted = await _context.UserLessonProgresses
            .CountAsync(p => p.UserId == userId && p.Status == "completed");
        var topicsCompleted = await _context.UserTopicProgresses
            .CountAsync(p => p.UserId == userId && p.Status == "completed");

        // Calculate streak
        var streak = await GetStreakAsync(userId);

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
            CurrentStreak = streak.CurrentStreak,
            LongestStreak = streak.LongestStreak,
            TotalStudyTimeMinutes = weeklyProgress.Sum(p => p.TimeSpentMinutes ?? 0),
            LessonsCompleted = lessonsCompleted,
            TopicsCompleted = topicsCompleted,
            WeeklyProgress = weeklyProgress.Select(p => new DailyProgressDto
            {
                Date = p.Date,
                QuestionsAnswered = p.QuestionsAnswered ?? 0,
                CorrectAnswers = p.QuestionsCorrect ?? 0,
                TimeSpentMinutes = p.TimeSpentMinutes ?? 0
            }).ToList()
        };
    }

    public async Task<MonthlyStatsDto> GetMonthlyStatsAsync(long userId)
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month;

        var monthProgress = await GetMonthlyProgressAsync(userId, year, month);

        var totalQuestions = monthProgress.Sum(p => p.QuestionsAnswered ?? 0);
        var correctQuestions = monthProgress.Sum(p => p.QuestionsCorrect ?? 0);
        var totalMinutes = monthProgress.Sum(p => p.TimeSpentMinutes ?? 0);
        var daysActive = monthProgress.Count(p => (p.QuestionsAnswered ?? 0) > 0);
        var totalXP = monthProgress.Sum(p => p.XpEarned ?? 0);

        // Get exam count for the month
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        var examCount = await _context.UserAttempts
            .Where(a => a.UserId == userId && a.Status == "completed" && 
                        a.SubmittedAt >= startDate.ToDateTime(TimeOnly.MinValue) && 
                        a.SubmittedAt <= endDate.ToDateTime(TimeOnly.MaxValue))
            .CountAsync();

        return new MonthlyStatsDto
        {
            Year = year,
            Month = month,
            MonthName = now.ToString("MMMM yyyy", new System.Globalization.CultureInfo("vi-VN")),
            TotalQuestions = totalQuestions,
            CorrectQuestions = correctQuestions,
            TotalMinutes = totalMinutes,
            TotalExams = examCount,
            TotalXP = totalXP,
            DaysActive = daysActive,
            AverageDailyXP = daysActive > 0 ? Math.Round((decimal)totalXP / daysActive, 1) : 0
        };
    }

    public async Task<StreakDto> GetStreakAsync(long userId)
    {
        // Get all daily progress records, ordered by date descending
        var allProgress = await _context.UserDailyProgresses
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.ProgressDate)
            .ToListAsync();

        if (!allProgress.Any())
        {
            return new StreakDto
            {
                CurrentStreak = 0,
                LongestStreak = 0,
                LastActivityDate = null
            };
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var yesterday = today.AddDays(-1);

        // Calculate current streak
        var currentStreak = 0;
        var lastActivityDate = allProgress.First().ProgressDate;

        // Check if today or yesterday has activity
        var hasActivityToday = allProgress.Any(p => p.ProgressDate == today && (p.QuestionsAnswered ?? 0) > 0);
        var hasActivityYesterday = allProgress.Any(p => p.ProgressDate == yesterday && (p.QuestionsAnswered ?? 0) > 0);

        if (hasActivityToday || hasActivityYesterday)
        {
            // Count consecutive days with activity
            var checkDate = hasActivityToday ? today : yesterday;
            foreach (var progress in allProgress.OrderByDescending(p => p.ProgressDate))
            {
                if (progress.ProgressDate == checkDate && (progress.QuestionsAnswered ?? 0) > 0)
                {
                    currentStreak++;
                    checkDate = checkDate.AddDays(-1);
                }
                else if (progress.ProgressDate < checkDate)
                {
                    break;
                }
            }
        }

        // Calculate longest streak
        var longestStreak = 0;
        var tempStreak = 0;
        var previousDate = (DateOnly?)null;

        foreach (var progress in allProgress.OrderBy(p => p.ProgressDate))
        {
            if ((progress.QuestionsAnswered ?? 0) > 0)
            {
                if (previousDate == null || progress.ProgressDate == previousDate.Value.AddDays(1))
                {
                    tempStreak++;
                }
                else
                {
                    tempStreak = 1;
                }
                previousDate = progress.ProgressDate;
                longestStreak = Math.Max(longestStreak, tempStreak);
            }
            else
            {
                tempStreak = 0;
                previousDate = null;
            }
        }

        return new StreakDto
        {
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            LastActivityDate = lastActivityDate.ToDateTime(TimeOnly.MinValue)
        };
    }

    #endregion

    #region XP Transaction

    public async Task AddXpTransactionAsync(long userId, int amount, string reason, string? sourceType = null, long? sourceId = null)
    {
        var transaction = new XpTransaction
        {
            UserId = userId,
            Amount = amount,
            Reason = reason,
            SourceType = sourceType,
            SourceId = sourceId,
            CreatedAt = DateTime.UtcNow
        };

        _context.XpTransactions.Add(transaction);

        // Update user total XP and level
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.TotalXp = (user.TotalXp ?? 0) + amount;
            user.CurrentLevel = CalculateLevel(user.TotalXp ?? 0);
            
            // Update streak
            var streak = await GetStreakAsync(userId);
            user.CurrentStreak = streak.CurrentStreak;
        }

        await _context.SaveChangesAsync();
    }

    private int CalculateLevel(int totalXp)
    {
        // Level calculation formula: Level = sqrt(TotalXP / 100)
        // Level 1: 0-99 XP
        // Level 2: 100-399 XP
        // Level 3: 400-899 XP
        // etc.
        if (totalXp < 100) return 1;
        return (int)Math.Floor(Math.Sqrt(totalXp / 100.0)) + 1;
    }

    #endregion
}

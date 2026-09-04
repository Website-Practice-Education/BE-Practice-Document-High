using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class LeaderboardService : ILeaderboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public LeaderboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<object>> GetGlobalLeaderboardAsync(int limit = 50)
    {
        var leaderboard = await _unitOfWork.Context.Set<User>()
            .Where(u => (u.IsActive == true || u.IsActive == null) && (u.TotalXp ?? 0) > 0)
            .OrderByDescending(u => u.TotalXp ?? 0)
            .Take(limit)
            .Select((u, index) => new
            {
                Rank = index + 1,
                u.Id,
                UserName = u.FullName,
                u.AvatarUrl,
                TotalXp = u.TotalXp ?? 0,
                Level = CalculateLevel(u.TotalXp ?? 0),
                Streak = u.CurrentStreak ?? 0,
                QuestionsAnswered = _unitOfWork.Context.Set<UserAnswer>()
                    .Count(ua => ua.Attempt != null && ua.Attempt.UserId == u.Id),
                ExamsCompleted = _unitOfWork.Context.Set<UserAttempt>()
                    .Count(ua => ua.UserId == u.Id && ua.Status == "completed")
            })
            .ToListAsync();

        return leaderboard.Cast<object>().ToList();
    }

    public async Task<List<object>> GetWeeklyLeaderboardAsync(int limit = 50)
    {
        var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        
        var leaderboard = await _unitOfWork.Context.Set<XpTransaction>()
            .Where(x => x.CreatedAt >= weekStart && x.Reason != "achievement")
            .GroupBy(x => x.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                WeeklyXp = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.WeeklyXp)
            .Take(limit)
            .ToListAsync();

        var result = new List<object>();
        var rank = 1;
        foreach (var item in leaderboard)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(item.UserId);
            if (user != null)
            {
                result.Add(new
                {
                    Rank = rank++,
                    UserId = user.Id,
                    UserName = user.FullName,
                    user.AvatarUrl,
                    TotalXp = user.TotalXp ?? 0,
                    Level = CalculateLevel(user.TotalXp ?? 0),
                    WeeklyXp = item.WeeklyXp,
                    Streak = user.CurrentStreak ?? 0
                });
            }
        }

        return result;
    }

    public async Task<List<object>> GetMonthlyLeaderboardAsync(int limit = 50)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        
        var leaderboard = await _unitOfWork.Context.Set<XpTransaction>()
            .Where(x => x.CreatedAt >= monthStart && x.Reason != "achievement")
            .GroupBy(x => x.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                MonthlyXp = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.MonthlyXp)
            .Take(limit)
            .ToListAsync();

        var result = new List<object>();
        var rank = 1;
        foreach (var item in leaderboard)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(item.UserId);
            if (user != null)
            {
                result.Add(new
                {
                    Rank = rank++,
                    UserId = user.Id,
                    UserName = user.FullName,
                    user.AvatarUrl,
                    TotalXp = user.TotalXp ?? 0,
                    Level = CalculateLevel(user.TotalXp ?? 0),
                    MonthlyXp = item.MonthlyXp,
                    Streak = user.CurrentStreak ?? 0
                });
            }
        }

        return result;
    }

    public async Task<object?> GetUserRankAsync(long userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return null;

        var totalXp = user.TotalXp ?? 0;
        var rank = await _unitOfWork.Context.Set<User>()
            .CountAsync(u => (u.IsActive == true || u.IsActive == null) && (u.TotalXp ?? 0) > totalXp);

        return new
        {
            UserId = user.Id,
            UserName = user.FullName,
            TotalXp = totalXp,
            Level = CalculateLevel(totalXp),
            Rank = rank + 1,
            Streak = user.CurrentStreak ?? 0
        };
    }

    public async Task<List<object>> GetTopUsersAsync(string type = "xp", int limit = 10)
    {
        return type.ToLower() switch
        {
            "streak" => await GetTopByStreakAsync(limit),
            "questions" => await GetTopByQuestionsAsync(limit),
            _ => (await GetGlobalLeaderboardAsync(limit)).Take(limit).ToList().Cast<object>().ToList()
        };
    }

    public async Task<int?> GetUserGlobalRankAsync(long userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return null;

        var totalXp = user.TotalXp ?? 0;
        return await _unitOfWork.Context.Set<User>()
            .CountAsync(u => (u.IsActive == true || u.IsActive == null) && (u.TotalXp ?? 0) > totalXp) + 1;
    }

    private async Task<List<object>> GetTopByStreakAsync(int limit)
    {
        var users = await _unitOfWork.Context.Set<User>()
            .Where(u => (u.IsActive == true || u.IsActive == null) && (u.CurrentStreak ?? 0) > 0)
            .OrderByDescending(u => u.CurrentStreak ?? 0)
            .Take(limit)
            .Select((u, index) => new
            {
                Rank = index + 1,
                u.Id,
                UserName = u.FullName,
                u.AvatarUrl,
                TotalXp = u.TotalXp ?? 0,
                Level = CalculateLevel(u.TotalXp ?? 0),
                Streak = u.CurrentStreak ?? 0
            })
            .ToListAsync();

        return users.Cast<object>().ToList();
    }

    private async Task<List<object>> GetTopByQuestionsAsync(int limit)
    {
        var results = await _unitOfWork.Context.Set<UserAnswer>()
            .GroupBy(ua => ua.Attempt != null ? ua.Attempt.UserId : 0)
            .Select(g => new
            {
                UserId = g.Key,
                QuestionsAnswered = g.Count(),
                CorrectAnswers = g.Count(ua => ua.IsCorrect == true)
            })
            .Where(x => x.UserId != 0)
            .OrderByDescending(x => x.QuestionsAnswered)
            .Take(limit)
            .ToListAsync();

        var result = new List<object>();
        var rank = 1;
        foreach (var item in results)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(item.UserId);
            if (user != null)
            {
                result.Add(new
                {
                    Rank = rank++,
                    UserId = user.Id,
                    UserName = user.FullName,
                    user.AvatarUrl,
                    TotalXp = user.TotalXp ?? 0,
                    Level = CalculateLevel(user.TotalXp ?? 0),
                    Streak = user.CurrentStreak ?? 0,
                    QuestionsAnswered = item.QuestionsAnswered,
                    CorrectAnswers = item.CorrectAnswers
                });
            }
        }

        return result;
    }

    private int CalculateLevel(int totalXp)
    {
        const int xpPerLevel = 500;
        return (totalXp / xpPerLevel) + 1;
    }
}

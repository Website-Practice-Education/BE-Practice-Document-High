using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website_Documents.API.Data;
using Website_Documents.API.DTOs;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class AchievementService : IAchievementService
{
    private readonly AppDbContext _context;

    public AchievementService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<object>> GetAllAchievementsAsync()
    {
        var achievements = await _context.Achievements
            .Where(a => a.IsActive)
            .OrderBy(a => a.ConditionValue)
            .Select(a => new
            {
                a.Id,
                a.Code,
                a.Name,
                a.Description,
                a.IconUrl,
                a.XpReward,
                a.ConditionType,
                a.ConditionValue,
                a.IsActive
            })
            .ToListAsync();

        return achievements.Cast<object>().ToList();
    }

    public async Task<List<object>> GetUserAchievementsAsync(long userId)
    {
        var userAchievements = await _context.UserAchievements
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId)
            .OrderByDescending(ua => ua.AchievedAt)
            .Select(ua => new
            {
                ua.AchievementId,
                AchievedAt = ua.AchievedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Achievement = new
                {
                    ua.Achievement.Id,
                    ua.Achievement.Code,
                    ua.Achievement.Name,
                    ua.Achievement.Description,
                    ua.Achievement.IconUrl,
                    ua.Achievement.XpReward,
                    ua.Achievement.ConditionType,
                    ua.Achievement.ConditionValue,
                    ua.Achievement.IsActive
                }
            })
            .ToListAsync();

        return userAchievements.Cast<object>().ToList();
    }

    public async Task<List<object>> CheckAndAwardAchievementsAsync(long userId)
    {
        var newlyUnlocked = new List<object>();
        
        // Get user stats
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return newlyUnlocked;

        // Get total questions answered
        var totalQuestions = await _context.UserAnswers
            .Where(ua => ua.Attempt.UserId == userId)
            .CountAsync();

        // Get exams completed
        var totalExams = await _context.UserAttempts
            .Where(ua => ua.UserId == userId && ua.Status == "completed")
            .CountAsync();

        // Get current streak
        var currentStreak = user.CurrentStreak;

        // Get spaces joined
        var spacesJoined = await _context.StudySpaceMembers
            .Where(sm => sm.UserId == userId)
            .CountAsync();

        // Get all available achievements not yet unlocked
        var unlockedCodes = await _context.UserAchievements
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.Achievement.Code)
            .ToListAsync();

        var availableAchievements = await _context.Achievements
            .Where(a => a.IsActive && !unlockedCodes.Contains(a.Code))
            .ToListAsync();

        foreach (var achievement in availableAchievements)
        {
            bool shouldUnlock = achievement.ConditionType switch
            {
                "questions_answered" => totalQuestions >= (achievement.ConditionValue ?? 0),
                "exams_completed" => totalExams >= (achievement.ConditionValue ?? 0),
                "streak_days" => currentStreak >= (achievement.ConditionValue ?? 0),
                "spaces_joined" => spacesJoined >= (achievement.ConditionValue ?? 0),
                _ => false
            };

            if (shouldUnlock)
            {
                await UnlockAchievementAsync(userId, achievement.Code);
                newlyUnlocked.Add(new
                {
                    achievement.Id,
                    achievement.Code,
                    achievement.Name,
                    achievement.Description,
                    achievement.XpReward
                });
            }
        }

        return newlyUnlocked;
    }

    public async Task<object?> UnlockAchievementAsync(long userId, string achievementCode)
    {
        var achievement = await _context.Achievements
            .FirstOrDefaultAsync(a => a.Code == achievementCode && a.IsActive);

        if (achievement == null) return null;

        // Check if already unlocked
        var existing = await _context.UserAchievements
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievement.Id);

        if (existing != null) return null;

        // Create user achievement
        var userAchievement = new UserAchievement
        {
            UserId = userId,
            AchievementId = achievement.Id,
            AchievedAt = DateTime.UtcNow
        };

        _context.UserAchievements.Add(userAchievement);

        // Add XP reward
        if (achievement.XpReward > 0)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.TotalXp = (user.TotalXp ?? 0) + achievement.XpReward;
                user.CurrentLevel = CalculateLevel(user.TotalXp ?? 0);
            }

            // Record XP transaction
            var transaction = new XpTransaction
            {
                UserId = userId,
                Amount = achievement.XpReward,
                Reason = "achievement",
                SourceType = "achievement",
                SourceId = achievement.Id,
                Description = $"Unlocked achievement: {achievement.Name}",
                CreatedAt = DateTime.UtcNow
            };
            _context.XpTransactions.Add(transaction);
        }

        await _context.SaveChangesAsync();

        return new
        {
            achievement.Id,
            achievement.Code,
            achievement.Name,
            achievement.Description,
            achievement.XpReward,
            AchievedAt = userAchievement.AchievedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    public async Task<int> CalculateTotalXPRewardsAsync(long userId)
    {
        return await _context.UserAchievements
            .Where(ua => ua.UserId == userId)
            .Include(ua => ua.Achievement)
            .SumAsync(ua => ua.Achievement.XpReward);
    }

    private int CalculateLevel(int totalXp)
    {
        const int xpPerLevel = 500;
        return (totalXp / xpPerLevel) + 1;
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class AchievementService : IAchievementService
{
    private readonly IUnitOfWork _unitOfWork;

    public AchievementService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<object>> GetAllAchievementsAsync()
    {
        var achievements = await _unitOfWork.Context.Set<Achievement>()
            .Where(a => a.IsActive == true || a.IsActive == null)
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
        var userAchievements = await _unitOfWork.Context.Set<UserAchievement>()
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId)
            .OrderByDescending(ua => ua.AchievedAt)
            .Select(ua => new
            {
                ua.AchievementId,
                AchievedAt = ua.AchievedAt.HasValue ? ua.AchievedAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null,
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
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return newlyUnlocked;

        // Get total questions answered
        var totalQuestions = await _unitOfWork.Context.Set<UserAnswer>()
            .Where(ua => ua.Attempt != null && ua.Attempt.UserId == userId)
            .CountAsync();

        // Get exams completed
        var totalExams = await _unitOfWork.Context.Set<UserAttempt>()
            .Where(ua => ua.UserId == userId && ua.Status == "completed")
            .CountAsync();

        // Get current streak
        var currentStreak = user.CurrentStreak ?? 0;

        // Get spaces joined
        var spacesJoined = await _unitOfWork.StudySpaceMembers.GetUserMembershipsAsync(userId);
        var spacesJoinedCount = spacesJoined.Count;

        // Get all available achievements not yet unlocked
        var unlockedCodes = await _unitOfWork.Context.Set<UserAchievement>()
            .Where(ua => ua.UserId == userId)
            .Include(ua => ua.Achievement)
            .Select(ua => ua.Achievement.Code)
            .ToListAsync();

        var availableAchievements = await _unitOfWork.Context.Set<Achievement>()
            .Where(a => (a.IsActive == true || a.IsActive == null) && !unlockedCodes.Contains(a.Code))
            .ToListAsync();

        foreach (var achievement in availableAchievements)
        {
            bool shouldUnlock = achievement.ConditionType switch
            {
                "questions_answered" => totalQuestions >= (achievement.ConditionValue ?? 0),
                "exams_completed" => totalExams >= (achievement.ConditionValue ?? 0),
                "streak_days" => currentStreak >= (achievement.ConditionValue ?? 0),
                "spaces_joined" => spacesJoinedCount >= (achievement.ConditionValue ?? 0),
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
        var achievement = await _unitOfWork.Context.Set<Achievement>()
            .FirstOrDefaultAsync(a => a.Code == achievementCode && (a.IsActive == true || a.IsActive == null));

        if (achievement == null) return null;

        // Check if already unlocked
        var existing = await _unitOfWork.Context.Set<UserAchievement>()
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievement.Id);

        if (existing != null) return null;

        // Create user achievement
        var userAchievement = new UserAchievement
        {
            UserId = userId,
            AchievementId = achievement.Id,
            AchievedAt = DateTime.UtcNow
        };

        _unitOfWork.Context.Set<UserAchievement>().Add(userAchievement);

        // Add XP reward
        if (achievement.XpReward > 0)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user != null)
            {
                user.TotalXp = (user.TotalXp ?? 0) + achievement.XpReward;
                user.CurrentLevel = CalculateLevel(user.TotalXp ?? 0);

                // Record XP transaction
                var transaction = new XpTransaction
                {
                    UserId = userId,
                    Amount = achievement.XpReward ?? 0,
                    Reason = "achievement",
                    SourceType = "achievement",
                    SourceId = achievement.Id,
                    Description = $"Unlocked achievement: {achievement.Name}",
                    CreatedAt = DateTime.UtcNow
                };
                _unitOfWork.Context.Set<XpTransaction>().Add(transaction);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return new
        {
            achievement.Id,
            achievement.Code,
            achievement.Name,
            achievement.Description,
            achievement.XpReward,
            AchievedAt = userAchievement.AchievedAt.HasValue ? userAchievement.AchievedAt.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null
        };
    }

    public async Task<int> CalculateTotalXPRewardsAsync(long userId)
    {
        return await _unitOfWork.Context.Set<UserAchievement>()
            .Where(ua => ua.UserId == userId)
            .Include(ua => ua.Achievement)
            .SumAsync(ua => ua.Achievement.XpReward ?? 0);
    }

    private int CalculateLevel(int totalXp)
    {
        const int xpPerLevel = 500;
        return (totalXp / xpPerLevel) + 1;
    }
}

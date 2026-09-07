using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class AchievementService : IAchievementService
{
    private readonly BookstoreDbContext _context;

    public AchievementService(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Achievement>> GetAllAchievementsAsync()
    {
        return await _context.Achievements
            .OrderBy(a => a.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Achievement>> GetActiveAchievementsAsync()
    {
        return await _context.Achievements
            .Where(a => a.IsActive == true)
            .OrderBy(a => a.Id)
            .ToListAsync();
    }

    public async Task<Achievement?> GetAchievementByIdAsync(int id)
    {
        return await _context.Achievements.FindAsync(id);
    }

    public async Task<Achievement?> GetAchievementByCodeAsync(string code)
    {
        return await _context.Achievements
            .FirstOrDefaultAsync(a => a.Code == code);
    }

    public async Task<IEnumerable<UserAchievement>> GetUserAchievementsAsync(long userId)
    {
        return await _context.UserAchievements
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId)
            .OrderByDescending(ua => ua.AchievedAt)
            .ToListAsync();
    }

    public async Task<bool> AwardAchievementAsync(long userId, int achievementId)
    {
        // Check if already awarded
        var existing = await _context.UserAchievements
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);

        if (existing != null)
            return false;

        var achievement = await _context.Achievements.FindAsync(achievementId);
        if (achievement == null)
            return false;

        var userAchievement = new UserAchievement
        {
            UserId = userId,
            AchievementId = achievementId,
            AchievedAt = DateTime.UtcNow
        };

        _context.UserAchievements.Add(userAchievement);

        // Update user XP
        var user = await _context.Users.FindAsync(userId);
        if (user != null && achievement.XpReward.HasValue)
        {
            user.TotalXp = (user.TotalXp ?? 0) + achievement.XpReward.Value;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Achievement>> CheckAndAwardAchievementsAsync(long userId)
    {
        var awardedAchievements = new List<Achievement>();
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return awardedAchievements;

        // Get all active achievements not yet awarded
        var userAchievementIds = await _context.UserAchievements
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.AchievementId)
            .ToListAsync();

        var availableAchievements = await _context.Achievements
            .Where(a => a.IsActive == true && !userAchievementIds.Contains(a.Id))
            .ToListAsync();

        foreach (var achievement in availableAchievements)
        {
            bool shouldAward = false;

            switch (achievement.ConditionType?.ToLower())
            {
                case "login_count":
                    // Check if user has logged in enough times
                    shouldAward = true; // Simplified - always award on check
                    break;

                case "questions_answered":
                    var questionsAnswered = await _context.UserAnswerHistories
                        .CountAsync(h => h.UserId == userId);
                    shouldAward = questionsAnswered >= (achievement.ConditionValue ?? 1);
                    break;

                case "exams_completed":
                    var examsCompleted = await _context.UserAttempts
                        .CountAsync(a => a.UserId == userId && a.Status == "submitted");
                    shouldAward = examsCompleted >= (achievement.ConditionValue ?? 1);
                    break;

                case "streak_days":
                    shouldAward = (user.CurrentStreak ?? 0) >= (achievement.ConditionValue ?? 1);
                    break;

                case "perfect_score":
                    var hasPerfectScore = await _context.UserAttempts
                        .AnyAsync(a => a.UserId == userId && a.Score == 100);
                    shouldAward = hasPerfectScore;
                    break;

                default:
                    // Award any achievement without specific condition
                    break;
            }

            if (shouldAward)
            {
                await AwardAchievementAsync(userId, achievement.Id);
                awardedAchievements.Add(achievement);
            }
        }

        return awardedAchievements;
    }
}

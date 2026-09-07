using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface IAchievementService
{
    Task<IEnumerable<Achievement>> GetAllAchievementsAsync();
    Task<IEnumerable<Achievement>> GetActiveAchievementsAsync();
    Task<Achievement?> GetAchievementByIdAsync(int id);
    Task<Achievement?> GetAchievementByCodeAsync(string code);
    Task<IEnumerable<UserAchievement>> GetUserAchievementsAsync(long userId);
    Task<bool> AwardAchievementAsync(long userId, int achievementId);
    Task<IEnumerable<Achievement>> CheckAndAwardAchievementsAsync(long userId);
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Website_Documents.Service.Interfaces;

public interface IAchievementService
{
    Task<List<object>> GetAllAchievementsAsync();
    Task<List<object>> GetUserAchievementsAsync(long userId);
    Task<List<object>> CheckAndAwardAchievementsAsync(long userId);
    Task<object?> UnlockAchievementAsync(long userId, string achievementCode);
    Task<int> CalculateTotalXPRewardsAsync(long userId);
}

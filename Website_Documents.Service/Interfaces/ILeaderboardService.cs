using System.Collections.Generic;
using System.Threading.Tasks;

namespace Website_Documents.Service.Interfaces;

public interface ILeaderboardService
{
    Task<List<object>> GetGlobalLeaderboardAsync(int limit = 50);
    Task<List<object>> GetWeeklyLeaderboardAsync(int limit = 50);
    Task<List<object>> GetMonthlyLeaderboardAsync(int limit = 50);
    Task<object?> GetUserRankAsync(long userId);
    Task<List<object>> GetTopUsersAsync(string type = "xp", int limit = 10);
    Task<int?> GetUserGlobalRankAsync(long userId);
}

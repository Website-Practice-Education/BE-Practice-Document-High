using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface IFriendshipService
{
    Task<Friendship> SendFriendRequestAsync(long userId, long friendId);
    Task<Friendship> AcceptFriendRequestAsync(long requestId, long userId);
    Task<Friendship> RejectFriendRequestAsync(long requestId, long userId);
    Task<bool> DeclineFriendRequestAsync(long requestId, long userId);
    Task<bool> RemoveFriendAsync(long userId, long friendId);
    Task<List<Friendship>> GetFriendsAsync(long userId);
    Task<List<Friendship>> GetPendingRequestsAsync(long userId);
    Task<List<Friendship>> SearchUsersAsync(long userId, string searchTerm);
    Task<bool> AreFriendsAsync(long userId, long friendId);
    Task<bool> BlockUserAsync(long userId, long blockUserId);
}

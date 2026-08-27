using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface IStudySpaceService
{
    Task<StudySpace> CreateSpaceAsync(long userId, string name, string? description, string spaceType = "public");
    Task<StudySpace?> GetSpaceByIdAsync(long spaceId);
    Task<StudySpace?> GetSpaceByInviteCodeAsync(string inviteCode);
    Task<List<StudySpace>> GetUserSpacesAsync(long userId);
    Task<List<StudySpace>> GetPublicSpacesAsync(int page = 1, int pageSize = 20);
    Task<bool> JoinSpaceAsync(long spaceId, long userId, string? inviteCode = null);
    Task<bool> LeaveSpaceAsync(long spaceId, long userId);
    Task<bool> IsMemberAsync(long spaceId, long userId);
    Task<List<StudySpaceMember>> GetSpaceMembersAsync(long spaceId);
    Task<bool> UpdateMemberRoleAsync(long spaceId, long userId, long targetUserId, string newRole);
    Task<bool> RemoveMemberAsync(long spaceId, long userId, long targetUserId);
    Task<string> GenerateInviteCodeAsync(long spaceId, long userId);
    Task<bool> DeleteSpaceAsync(long spaceId, long userId);
}

public interface IChatService
{
    Task<ChatMessage> SendMessageAsync(long spaceId, long userId, string content, string messageType = "text");
    Task<List<ChatMessage>> GetMessagesAsync(long spaceId, int page = 1, int pageSize = 50);
    Task<int> GetUnreadCountAsync(long spaceId, long userId, DateTime? since = null);
}

public interface IFriendshipService
{
    Task<Friendship> SendFriendRequestAsync(long userId, long friendId);
    Task<Friendship?> AcceptFriendRequestAsync(long requestId, long userId);
    Task<bool> DeclineFriendRequestAsync(long requestId, long userId);
    Task<bool> RemoveFriendAsync(long userId, long friendId);
    Task<bool> BlockUserAsync(long userId, long blockUserId);
    Task<List<Friendship>> GetFriendsAsync(long userId);
    Task<List<Friendship>> GetPendingRequestsAsync(long userId);
    Task<List<Friendship>> SearchUsersAsync(long userId, string searchTerm);
    Task<bool> AreFriendsAsync(long userId1, long userId2);
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository.Interfaces;

public interface IStudySpaceRepository
{
    Task<StudySpace?> GetByIdAsync(long id);
    Task<StudySpace?> GetByInviteCodeAsync(string inviteCode);
    Task<List<StudySpace>> GetUserSpacesAsync(long userId);
    Task<List<StudySpace>> GetPublicSpacesAsync(int page, int pageSize);
    Task<StudySpace> CreateAsync(StudySpace space);
    Task UpdateAsync(StudySpace space);
    Task DeleteAsync(long id);
}

public interface IStudySpaceMemberRepository
{
    Task<StudySpaceMember?> GetByIdAsync(long id);
    Task<StudySpaceMember?> GetMemberAsync(long spaceId, long userId);
    Task<List<StudySpaceMember>> GetSpaceMembersAsync(long spaceId);
    Task<List<StudySpaceMember>> GetUserMembershipsAsync(long userId);
    Task<StudySpaceMember> CreateAsync(StudySpaceMember member);
    Task UpdateAsync(StudySpaceMember member);
    Task DeleteAsync(long id);
    Task<bool> IsMemberAsync(long spaceId, long userId);
}

public interface IChatMessageRepository
{
    Task<ChatMessage> CreateAsync(ChatMessage message);
    Task<ChatMessage?> GetByIdAsync(long id);
    Task<List<ChatMessage>> GetMessagesAsync(long spaceId, int page, int pageSize);
    Task<int> GetUnreadCountAsync(long spaceId, long userId, System.DateTime? since = null);
}

public interface IFriendshipRepository
{
    Task<Friendship?> GetByIdAsync(long id);
    Task<Friendship?> GetFriendshipAsync(long userId, long friendId);
    Task<List<Friendship>> GetFriendsAsync(long userId);
    Task<List<Friendship>> GetPendingRequestsAsync(long userId);
    Task<List<Friendship>> SearchUsersAsync(long userId, string searchTerm);
    Task<Friendship> CreateAsync(Friendship friendship);
    Task UpdateAsync(Friendship friendship);
    Task DeleteAsync(long id);
    Task<bool> AreFriendsAsync(long userId1, long userId2);
}

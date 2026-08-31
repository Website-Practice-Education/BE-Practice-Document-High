using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class StudySpaceService : IStudySpaceService
{
    private readonly IUnitOfWork _unitOfWork;

    public StudySpaceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StudySpace> CreateSpaceAsync(long userId, string name, string? description, string spaceType = "public")
    {
        var inviteCode = GenerateInviteCode();
        
        var space = new StudySpace
        {
            Name = name,
            Description = description,
            SpaceType = spaceType,
            InviteCode = inviteCode,
            MaxMembers = 50,
            IsActive = true,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            var createdSpace = await _unitOfWork.StudySpaces.CreateAsync(space);

            var member = new StudySpaceMember
            {
                SpaceId = createdSpace.Id,
                UserId = userId,
                Role = "owner",
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _unitOfWork.StudySpaceMembers.CreateAsync(member);

            await _unitOfWork.CommitTransactionAsync();
            
            return createdSpace;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw new InvalidOperationException($"Failed to create study space: {ex.Message}", ex);
        }
    }

    public async Task<StudySpace?> GetSpaceByIdAsync(long spaceId)
    {
        return await _unitOfWork.StudySpaces.GetByIdAsync(spaceId);
    }

    public async Task<StudySpace?> GetSpaceByInviteCodeAsync(string inviteCode)
    {
        return await _unitOfWork.StudySpaces.GetByInviteCodeAsync(inviteCode);
    }

    public async Task<List<StudySpace>> GetUserSpacesAsync(long userId)
    {
        return await _unitOfWork.StudySpaces.GetUserSpacesAsync(userId);
    }

    public async Task<List<StudySpace>> GetPublicSpacesAsync(int page = 1, int pageSize = 20)
    {
        return await _unitOfWork.StudySpaces.GetPublicSpacesAsync(page, pageSize);
    }

    public async Task<bool> JoinSpaceAsync(long spaceId, long userId, string? inviteCode = null)
    {
        var space = await _unitOfWork.StudySpaces.GetByIdAsync(spaceId);
        if (space == null || !space.IsActive == true)
            return false;

        if (space.SpaceType == "private" && space.InviteCode != inviteCode)
            return false;

        var isMember = await _unitOfWork.StudySpaceMembers.IsMemberAsync(spaceId, userId);
        if (isMember)
            return true;

        if (space.Members.Count >= space.MaxMembers)
            return false;

        var member = new StudySpaceMember
        {
            SpaceId = spaceId,
            UserId = userId,
            Role = "member",
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };
        await _unitOfWork.StudySpaceMembers.CreateAsync(member);
        return true;
    }

    public async Task<bool> LeaveSpaceAsync(long spaceId, long userId)
    {
        var member = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, userId);
        if (member == null)
            return false;

        if (member.Role == "owner")
            return false;

        await _unitOfWork.StudySpaceMembers.DeleteAsync(member.Id);
        return true;
    }

    public async Task<bool> IsMemberAsync(long spaceId, long userId)
    {
        return await _unitOfWork.StudySpaceMembers.IsMemberAsync(spaceId, userId);
    }

    public async Task<List<StudySpaceMember>> GetSpaceMembersAsync(long spaceId)
    {
        return await _unitOfWork.StudySpaceMembers.GetSpaceMembersAsync(spaceId);
    }

    public async Task<bool> UpdateMemberRoleAsync(long spaceId, long userId, long targetUserId, string newRole)
    {
        var requester = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, userId);
        if (requester == null || requester.Role != "owner")
            return false;

        var target = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, targetUserId);
        if (target == null)
            return false;

        target.Role = newRole;
        await _unitOfWork.StudySpaceMembers.UpdateAsync(target);
        return true;
    }

    public async Task<bool> RemoveMemberAsync(long spaceId, long userId, long targetUserId)
    {
        var requester = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, userId);
        if (requester == null || (requester.Role != "owner" && requester.Role != "admin"))
            return false;

        var target = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, targetUserId);
        if (target == null)
            return false;

        if (target.Role == "owner")
            return false;

        await _unitOfWork.StudySpaceMembers.DeleteAsync(target.Id);
        return true;
    }

    public async Task<string> GenerateInviteCodeAsync(long spaceId, long userId)
    {
        var member = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, userId);
        if (member == null || member.Role != "owner")
            return string.Empty;

        var space = await _unitOfWork.StudySpaces.GetByIdAsync(spaceId);
        if (space == null)
            return string.Empty;

        space.InviteCode = GenerateInviteCode();
        await _unitOfWork.StudySpaces.UpdateAsync(space);
        return space.InviteCode;
    }

    public async Task<bool> DeleteSpaceAsync(long spaceId, long userId)
    {
        var member = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, userId);
        if (member == null || member.Role != "owner")
            return false;

        await _unitOfWork.StudySpaces.DeleteAsync(spaceId);
        return true;
    }

    public async Task<bool> UpdateSpaceAsync(long spaceId, long userId, string? name, string? description, string? spaceType)
    {
        var member = await _unitOfWork.StudySpaceMembers.GetMemberAsync(spaceId, userId);
        if (member == null || member.Role != "owner")
            return false;

        var space = await _unitOfWork.StudySpaces.GetByIdAsync(spaceId);
        if (space == null)
            return false;

        if (!string.IsNullOrWhiteSpace(name))
            space.Name = name;
        if (description != null)
            space.Description = description;
        if (!string.IsNullOrWhiteSpace(spaceType))
            space.SpaceType = spaceType;

        await _unitOfWork.StudySpaces.UpdateAsync(space);
        return true;
    }

    private string GenerateInviteCode()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpper();
    }
}

public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;

    public ChatService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ChatMessage> SendMessageAsync(long spaceId, long userId, string content, string messageType = "text")
    {
        var message = new ChatMessage
        {
            SpaceId = spaceId,
            UserId = userId,
            Content = content,
            MessageType = messageType,
            CreatedAt = DateTime.UtcNow
        };

        var createdMessage = await _unitOfWork.ChatMessages.CreateAsync(message);
        
        // Reload with User included for SignalR broadcast
        return await _unitOfWork.ChatMessages.GetByIdAsync(createdMessage.Id) ?? createdMessage;
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(long spaceId, int page = 1, int pageSize = 50)
    {
        return await _unitOfWork.ChatMessages.GetMessagesAsync(spaceId, page, pageSize);
    }

    public async Task<int> GetUnreadCountAsync(long spaceId, long userId, DateTime? since = null)
    {
        return await _unitOfWork.ChatMessages.GetUnreadCountAsync(spaceId, userId, since);
    }
}

public class FriendshipService : IFriendshipService
{
    private readonly IUnitOfWork _unitOfWork;

    public FriendshipService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Friendship> SendFriendRequestAsync(long userId, long friendId)
    {
        if (userId == friendId)
            throw new InvalidOperationException("Cannot send friend request to yourself");

        var existing = await _unitOfWork.Friendships.GetFriendshipAsync(userId, friendId);
        if (existing != null)
            throw new InvalidOperationException("Friendship or request already exists");

        var friendship = new Friendship
        {
            UserId = userId,
            FriendId = friendId,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _unitOfWork.Friendships.CreateAsync(friendship);
    }

    public async Task<Friendship> AcceptFriendRequestAsync(long requestId, long userId)
    {
        var friendship = await _unitOfWork.Friendships.GetByIdAsync(requestId);
        if (friendship == null || friendship.FriendId != userId || friendship.Status != "pending")
            throw new InvalidOperationException("Invalid friend request");

        friendship.Status = "accepted";
        friendship.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Friendships.UpdateAsync(friendship);
        return friendship;
    }

    public async Task<bool> DeclineFriendRequestAsync(long requestId, long userId)
    {
        try
        {
            await RejectFriendRequestAsync(requestId, userId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Friendship> RejectFriendRequestAsync(long requestId, long userId)
    {
        var friendship = await _unitOfWork.Friendships.GetByIdAsync(requestId);
        if (friendship == null || friendship.FriendId != userId || friendship.Status != "pending")
            throw new InvalidOperationException("Invalid friend request");

        await _unitOfWork.Friendships.DeleteAsync(requestId);
        return friendship;
    }

    public async Task<bool> RemoveFriendAsync(long userId, long friendId)
    {
        var friendship = await _unitOfWork.Friendships.GetFriendshipAsync(userId, friendId);
        if (friendship == null || friendship.Status != "accepted")
            return false;

        await _unitOfWork.Friendships.DeleteAsync(friendship.Id);
        return true;
    }

    public async Task<bool> BlockUserAsync(long userId, long blockUserId)
    {
        var friendship = await _unitOfWork.Friendships.GetFriendshipAsync(userId, blockUserId);
        if (friendship != null)
        {
            friendship.Status = "blocked";
            friendship.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Friendships.UpdateAsync(friendship);
        }
        else
        {
            var block = new Friendship
            {
                UserId = userId,
                FriendId = blockUserId,
                Status = "blocked",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Friendships.CreateAsync(block);
        }
        return true;
    }

    public async Task<List<Friendship>> GetFriendsAsync(long userId)
    {
        return await _unitOfWork.Friendships.GetFriendsAsync(userId);
    }

    public async Task<List<Friendship>> GetPendingRequestsAsync(long userId)
    {
        return await _unitOfWork.Friendships.GetPendingRequestsAsync(userId);
    }

    public async Task<List<Friendship>> SearchUsersAsync(long userId, string searchTerm)
    {
        return await _unitOfWork.Friendships.SearchUsersAsync(userId, searchTerm);
    }

    public async Task<bool> AreFriendsAsync(long userId1, long userId2)
    {
        return await _unitOfWork.Friendships.AreFriendsAsync(userId1, userId2);
    }
}

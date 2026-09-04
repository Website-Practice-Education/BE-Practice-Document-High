using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service;

public class FriendshipService : Interfaces.IFriendshipService
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

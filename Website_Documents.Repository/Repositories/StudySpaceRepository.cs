using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository;

public class StudySpaceRepository : IStudySpaceRepository
{
    private readonly BookstoreDbContext _context;

    public StudySpaceRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<StudySpace?> GetByIdAsync(long id)
    {
        return await _context.StudySpaces
            .Include(s => s.Members)
            .Include(s => s.Creator)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<StudySpace?> GetByInviteCodeAsync(string inviteCode)
    {
        return await _context.StudySpaces
            .Include(s => s.Members)
            .FirstOrDefaultAsync(s => s.InviteCode == inviteCode && s.IsActive == true);
    }

    public async Task<List<StudySpace>> GetUserSpacesAsync(long userId)
    {
        return await _context.StudySpaces
            .Include(s => s.Members.Where(m => m.UserId == userId))
            .Include(s => s.Creator)
            .Where(s => s.Members.Any(m => m.UserId == userId && m.IsActive == true))
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<StudySpace>> GetPublicSpacesAsync(int page, int pageSize)
    {
        return await _context.StudySpaces
            .Include(s => s.Members)
            .Include(s => s.Creator)
            .Where(s => s.SpaceType == "public" && s.IsActive == true)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<StudySpace> CreateAsync(StudySpace space)
    {
        _context.StudySpaces.Add(space);
        await _context.SaveChangesAsync();
        return space;
    }

    public async Task UpdateAsync(StudySpace space)
    {
        // Detach any tracked entity with the same ID to avoid conflicts
        var tracked = _context.StudySpaces.Local.FirstOrDefault(e => e.Id == space.Id);
        if (tracked != null)
            _context.Entry(tracked).State = EntityState.Detached;

        // Attach and only mark scalar properties as modified (not navigation properties)
        _context.StudySpaces.Attach(space);
        _context.Entry(space).Property(x => x.Name).IsModified = true;
        _context.Entry(space).Property(x => x.Description).IsModified = true;
        _context.Entry(space).Property(x => x.SpaceType).IsModified = true;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var space = await _context.StudySpaces.FindAsync(id);
        if (space != null)
        {
            space.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}

public class StudySpaceMemberRepository : IStudySpaceMemberRepository
{
    private readonly BookstoreDbContext _context;

    public StudySpaceMemberRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<StudySpaceMember?> GetByIdAsync(long id)
    {
        return await _context.StudySpaceMembers
            .Include(m => m.User)
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<StudySpaceMember?> GetMemberAsync(long spaceId, long userId)
    {
        return await _context.StudySpaceMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.SpaceId == spaceId && m.UserId == userId);
    }

    public async Task<List<StudySpaceMember>> GetSpaceMembersAsync(long spaceId)
    {
        return await _context.StudySpaceMembers
            .Include(m => m.User)
            .Where(m => m.SpaceId == spaceId && m.IsActive == true)
            .OrderByDescending(m => m.JoinedAt)
            .ToListAsync();
    }

    public async Task<List<StudySpaceMember>> GetUserMembershipsAsync(long userId)
    {
        return await _context.StudySpaceMembers
            .Include(m => m.Space)
            .Where(m => m.UserId == userId && m.IsActive == true)
            .ToListAsync();
    }

    public async Task<StudySpaceMember> CreateAsync(StudySpaceMember member)
    {
        _context.StudySpaceMembers.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task UpdateAsync(StudySpaceMember member)
    {
        _context.StudySpaceMembers.Update(member);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var member = await _context.StudySpaceMembers.FindAsync(id);
        if (member != null)
        {
            member.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsMemberAsync(long spaceId, long userId)
    {
        return await _context.StudySpaceMembers
            .AnyAsync(m => m.SpaceId == spaceId && m.UserId == userId && m.IsActive == true);
    }
}

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly BookstoreDbContext _context;

    public ChatMessageRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<ChatMessage> CreateAsync(ChatMessage message)
    {
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<ChatMessage?> GetByIdAsync(long id)
    {
        return await _context.ChatMessages
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(long spaceId, int page, int pageSize)
    {
        return await _context.ChatMessages
            .Include(m => m.User)
            .Where(m => m.SpaceId == spaceId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(long spaceId, long userId, DateTime? since = null)
    {
        var query = _context.ChatMessages
            .Where(m => m.SpaceId == spaceId && m.UserId != userId);

        if (since.HasValue)
        {
            query = query.Where(m => m.CreatedAt > since.Value);
        }

        return await query.CountAsync();
    }
}

public class FriendshipRepository : IFriendshipRepository
{
    private readonly BookstoreDbContext _context;

    public FriendshipRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<Friendship?> GetByIdAsync(long id)
    {
        return await _context.Friendships
            .Include(f => f.User)
            .Include(f => f.Friend)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Friendship?> GetFriendshipAsync(long userId, long friendId)
    {
        return await _context.Friendships
            .FirstOrDefaultAsync(f => 
                (f.UserId == userId && f.FriendId == friendId) || 
                (f.UserId == friendId && f.FriendId == userId));
    }

    public async Task<List<Friendship>> GetFriendsAsync(long userId)
    {
        return await _context.Friendships
            .Include(f => f.User)
            .Include(f => f.Friend)
            .Where(f => (f.UserId == userId || f.FriendId == userId) && f.Status == "accepted")
            .ToListAsync();
    }

    public async Task<List<Friendship>> GetPendingRequestsAsync(long userId)
    {
        return await _context.Friendships
            .Include(f => f.User)
            .Include(f => f.Friend)
            .Where(f => f.FriendId == userId && f.Status == "pending")
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Friendship>> SearchUsersAsync(long userId, string searchTerm)
    {
        return await _context.Users
            .Where(u => u.Id != userId && u.IsActive == true && 
                (u.FullName != null && u.FullName.Contains(searchTerm) || u.Email.Contains(searchTerm)))
            .Take(20)
            .Select(u => new Friendship 
            { 
                FriendId = u.Id, 
                Friend = u,
                UserId = userId,
                Status = "search_result"
            })
            .ToListAsync();
    }

    public async Task<Friendship> CreateAsync(Friendship friendship)
    {
        _context.Friendships.Add(friendship);
        await _context.SaveChangesAsync();
        return friendship;
    }

    public async Task UpdateAsync(Friendship friendship)
    {
        _context.Friendships.Update(friendship);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var friendship = await _context.Friendships.FindAsync(id);
        if (friendship != null)
        {
            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> AreFriendsAsync(long userId1, long userId2)
    {
        return await _context.Friendships
            .AnyAsync(f => 
                ((f.UserId == userId1 && f.FriendId == userId2) || 
                (f.UserId == userId2 && f.FriendId == userId1)) && 
                f.Status == "accepted");
    }
}

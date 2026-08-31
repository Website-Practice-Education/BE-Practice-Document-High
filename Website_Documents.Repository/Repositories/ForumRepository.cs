using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository.Repositories;

public class ForumRepository : IForumRepository
{
    private readonly BookstoreDbContext _context;

    public ForumRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<(List<ForumPost> Posts, int TotalCount)> GetPostsAsync(int page, int pageSize)
    {
        var query = _context.ForumPosts
            .Include(p => p.User)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (posts, totalCount);
    }

    public async Task<ForumPost?> GetPostByIdAsync(int id)
    {
        return await _context.ForumPosts
            .Include(p => p.User)
            .Include(p => p.Comments.Where(c => !c.IsDeleted).OrderByDescending(c => c.CreatedAt).Take(5))
                .ThenInclude(c => c.User)
            .Include(p => p.Likes)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<ForumPost> CreatePostAsync(ForumPost post)
    {
        post.CreatedAt = DateTime.UtcNow;
        _context.ForumPosts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<ForumPost?> UpdatePostAsync(int id, string content, string? documentUrl, string? sharedLink)
    {
        var post = await _context.ForumPosts.FindAsync(id);
        if (post == null || post.IsDeleted) return null;

        post.Content = content;
        post.DocumentUrl = documentUrl;
        post.SharedLink = sharedLink;
        post.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<bool> DeletePostAsync(int id)
    {
        var post = await _context.ForumPosts.FindAsync(id);
        if (post == null) return false;

        post.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ForumComment?> AddCommentAsync(int postId, long userId, string content)
    {
        var post = await _context.ForumPosts.FindAsync(postId);
        if (post == null || post.IsDeleted) return null;

        var comment = new ForumComment
        {
            PostId = postId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        _context.ForumComments.Add(comment);
        post.CommentCount++;
        await _context.SaveChangesAsync();

        // Reload with user
        return await _context.ForumComments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == comment.Id);
    }

    public async Task<List<ForumComment>> GetCommentsAsync(int postId, int page, int pageSize)
    {
        return await _context.ForumComments
            .Include(c => c.User)
            .Where(c => c.PostId == postId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        var comment = await _context.ForumComments.FindAsync(commentId);
        if (comment == null || comment.IsDeleted) return false;

        comment.IsDeleted = true;
        
        var post = await _context.ForumPosts.FindAsync(comment.PostId);
        if (post != null && post.CommentCount > 0)
            post.CommentCount--;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleLikeAsync(int postId, long userId)
    {
        var existingLike = await _context.ForumLikes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

        var post = await _context.ForumPosts.FindAsync(postId);
        if (post == null) return false;

        if (existingLike != null)
        {
            _context.ForumLikes.Remove(existingLike);
            post.LikeCount = Math.Max(0, post.LikeCount - 1);
        }
        else
        {
            _context.ForumLikes.Add(new ForumLike
            {
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            post.LikeCount++;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsLikedByUserAsync(int postId, long userId)
    {
        return await _context.ForumLikes
            .AnyAsync(l => l.PostId == postId && l.UserId == userId);
    }

    public async Task<List<int>> GetLikedPostIdsAsync(long userId, List<int> postIds)
    {
        return await _context.ForumLikes
            .Where(l => l.UserId == userId && postIds.Contains(l.PostId))
            .Select(l => l.PostId)
            .ToListAsync();
    }
}

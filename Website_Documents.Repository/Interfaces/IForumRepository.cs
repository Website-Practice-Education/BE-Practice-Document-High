using Website_Documents.Repository.Models;

namespace Website_Documents.Repository.Interfaces;

public interface IForumRepository
{
    Task<(List<ForumPost> Posts, int TotalCount)> GetPostsAsync(int page, int pageSize);
    Task<ForumPost?> GetPostByIdAsync(int id);
    Task<ForumPost> CreatePostAsync(ForumPost post);
    Task<ForumPost?> UpdatePostAsync(int id, string content, string? documentUrl, string? sharedLink);
    Task<bool> DeletePostAsync(int id);
    
    Task<ForumComment?> AddCommentAsync(int postId, long userId, string content);
    Task<List<ForumComment>> GetCommentsAsync(int postId, int page, int pageSize);
    Task<bool> DeleteCommentAsync(int commentId);
    
    Task<bool> ToggleLikeAsync(int postId, long userId);
    Task<bool> IsLikedByUserAsync(int postId, long userId);
    Task<List<int>> GetLikedPostIdsAsync(long userId, List<int> postIds);
}

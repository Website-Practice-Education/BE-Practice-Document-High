using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface IForumService
{
    Task<(List<ForumPostDto> Posts, int TotalCount, int TotalPages)> GetPostsAsync(int page, int pageSize, long currentUserId);
    Task<ForumPostDetailDto?> GetPostByIdAsync(int id, long currentUserId);
    Task<ForumPostDto> CreatePostAsync(long userId, string content, string? documentUrl, string? sharedLink);
    Task<ForumPostDto?> UpdatePostAsync(int id, long userId, string content, string? documentUrl, string? sharedLink);
    Task<bool> DeletePostAsync(int id, long userId);
    
    Task<ForumCommentDto?> AddCommentAsync(int postId, long userId, string content);
    Task<List<ForumCommentDto>> GetCommentsAsync(int postId, int page, int pageSize);
    Task<bool> DeleteCommentAsync(int commentId, long userId);
    
    Task<bool> ToggleLikeAsync(int postId, long userId);
}

public class ForumPostDto
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? DocumentUrl { get; set; }
    public string? SharedLink { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsLiked { get; set; }
    public bool IsOwner { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}

public class ForumPostDetailDto : ForumPostDto
{
    public List<ForumCommentDto> Comments { get; set; } = new();
}

public class ForumCommentDto
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}

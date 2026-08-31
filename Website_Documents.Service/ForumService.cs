using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace Website_Documents.Service;

public class ForumService : IForumService
{
    private readonly IForumRepository _forumRepository;
    private readonly ILogger<ForumService> _logger;

    public ForumService(IForumRepository forumRepository, ILogger<ForumService> logger)
    {
        _forumRepository = forumRepository;
        _logger = logger;
    }

    public async Task<(List<ForumPostDto> Posts, int TotalCount, int TotalPages)> GetPostsAsync(int page, int pageSize, long currentUserId)
    {
        var (posts, totalCount) = await _forumRepository.GetPostsAsync(page, pageSize);
        var postIds = posts.Select(p => p.Id).ToList();
        var likedIds = await _forumRepository.GetLikedPostIdsAsync(currentUserId, postIds);

        var dtos = posts.Select(p => MapToDto(p, currentUserId, likedIds.Contains(p.Id))).ToList();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return (dtos, totalCount, totalPages);
    }

    public async Task<ForumPostDetailDto?> GetPostByIdAsync(int id, long currentUserId)
    {
        var post = await _forumRepository.GetPostByIdAsync(id);
        if (post == null) return null;

        var isLiked = await _forumRepository.IsLikedByUserAsync(id, currentUserId);
        var dto = MapToDetailDto(post, currentUserId, isLiked);

        var comments = await _forumRepository.GetCommentsAsync(id, 1, 50);
        dto.Comments = comments.Select(c => MapCommentToDto(c, currentUserId)).ToList();

        return dto;
    }

    public async Task<ForumPostDto> CreatePostAsync(long userId, string content, string? documentUrl, string? sharedLink)
    {
        var post = new ForumPost
        {
            UserId = userId,
            Content = content,
            DocumentUrl = documentUrl,
            SharedLink = sharedLink
        };

        var created = await _forumRepository.CreatePostAsync(post);
        return MapToDto(created, userId, false);
    }

    public async Task<ForumPostDto?> UpdatePostAsync(int id, long userId, string content, string? documentUrl, string? sharedLink)
    {
        var post = await _forumRepository.GetPostByIdAsync(id);
        if (post == null || post.UserId != userId) return null;

        var updated = await _forumRepository.UpdatePostAsync(id, content, documentUrl, sharedLink);
        if (updated == null) return null;

        var isLiked = await _forumRepository.IsLikedByUserAsync(id, userId);
        return MapToDto(updated, userId, isLiked);
    }

    public async Task<bool> DeletePostAsync(int id, long userId)
    {
        var post = await _forumRepository.GetPostByIdAsync(id);
        if (post == null || post.UserId != userId) return false;

        return await _forumRepository.DeletePostAsync(id);
    }

    public async Task<ForumCommentDto?> AddCommentAsync(int postId, long userId, string content)
    {
        var comment = await _forumRepository.AddCommentAsync(postId, userId, content);
        if (comment == null) return null;

        return MapCommentToDto(comment, userId);
    }

    public async Task<List<ForumCommentDto>> GetCommentsAsync(int postId, int page, int pageSize)
    {
        var comments = await _forumRepository.GetCommentsAsync(postId, page, pageSize);
        // Assuming current user for this context - in real app, pass userId
        return comments.Select(c => MapCommentToDto(c, 0)).ToList();
    }

    public async Task<bool> DeleteCommentAsync(int commentId, long userId)
    {
        // In a full implementation, you'd verify the user owns the comment
        return await _forumRepository.DeleteCommentAsync(commentId);
    }

    public async Task<bool> ToggleLikeAsync(int postId, long userId)
    {
        return await _forumRepository.ToggleLikeAsync(postId, userId);
    }

    private static ForumPostDto MapToDto(ForumPost post, long currentUserId, bool isLiked)
    {
        return new ForumPostDto
        {
            Id = post.Id,
            UserId = post.UserId,
            UserName = post.User?.FullName ?? post.User?.Email ?? "Unknown",
            UserAvatar = post.User?.AvatarUrl,
            Content = post.Content,
            DocumentUrl = post.DocumentUrl,
            SharedLink = post.SharedLink,
            LikeCount = post.LikeCount,
            CommentCount = post.CommentCount,
            IsLiked = isLiked,
            IsOwner = post.UserId == currentUserId,
            CreatedAt = post.CreatedAt,
            TimeAgo = GetTimeAgo(post.CreatedAt)
        };
    }

    private static ForumPostDetailDto MapToDetailDto(ForumPost post, long currentUserId, bool isLiked)
    {
        return new ForumPostDetailDto
        {
            Id = post.Id,
            UserId = post.UserId,
            UserName = post.User?.FullName ?? post.User?.Email ?? "Unknown",
            UserAvatar = post.User?.AvatarUrl,
            Content = post.Content,
            DocumentUrl = post.DocumentUrl,
            SharedLink = post.SharedLink,
            LikeCount = post.LikeCount,
            CommentCount = post.CommentCount,
            IsLiked = isLiked,
            IsOwner = post.UserId == currentUserId,
            CreatedAt = post.CreatedAt,
            TimeAgo = GetTimeAgo(post.CreatedAt)
        };
    }

    private static ForumCommentDto MapCommentToDto(ForumComment comment, long currentUserId)
    {
        return new ForumCommentDto
        {
            Id = comment.Id,
            UserId = comment.UserId,
            UserName = comment.User?.FullName ?? comment.User?.Email ?? "Unknown",
            UserAvatar = comment.User?.AvatarUrl,
            Content = comment.Content,
            IsOwner = comment.UserId == currentUserId,
            CreatedAt = comment.CreatedAt,
            TimeAgo = GetTimeAgo(comment.CreatedAt)
        };
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalSeconds < 60)
            return "Vừa xong";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} phút trước";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} giờ trước";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} ngày trước";
        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} tuần trước";
        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} tháng trước";

        return $"{(int)(timeSpan.TotalDays / 365)} năm trước";
    }
}

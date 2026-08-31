using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website_Documents.Service.Interfaces;
using System.Security.Claims;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ForumController : ControllerBase
{
    private readonly IForumService _forumService;
    private readonly ILogger<ForumController> _logger;

    public ForumController(IForumService forumService, ILogger<ForumController> logger)
    {
        _forumService = forumService;
        _logger = logger;
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("nameidentifier")?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Get paginated list of forum posts
    /// </summary>
    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (posts, totalCount, totalPages) = await _forumService.GetPostsAsync(page, pageSize, userId);
            
            return Ok(new
            {
                success = true,
                data = posts,
                pagination = new
                {
                    currentPage = page,
                    pageSize,
                    totalCount,
                    totalPages
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting forum posts");
            return StatusCode(500, new { success = false, message = "Lỗi khi lấy danh sách bài viết" });
        }
    }

    /// <summary>
    /// Get a single post with comments
    /// </summary>
    [HttpGet("posts/{id}")]
    public async Task<IActionResult> GetPost(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var post = await _forumService.GetPostByIdAsync(id, userId);
            
            if (post == null)
                return NotFound(new { success = false, message = "Bài viết không tồn tại" });

            return Ok(new { success = true, data = post });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting forum post {PostId}", id);
            return StatusCode(500, new { success = false, message = "Lỗi khi lấy bài viết" });
        }
    }

    /// <summary>
    /// Create a new forum post
    /// </summary>
    [HttpPost("posts")]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });

            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { success = false, message = "Nội dung không được trống" });

            var post = await _forumService.CreatePostAsync(userId, request.Content.Trim(), request.DocumentUrl, request.SharedLink);
            
            return Ok(new { success = true, data = post, message = "Đăng bài thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating forum post");
            return StatusCode(500, new { success = false, message = "Lỗi khi đăng bài" });
        }
    }

    /// <summary>
    /// Update a forum post
    /// </summary>
    [HttpPut("posts/{id}")]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });

            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { success = false, message = "Nội dung không được trống" });

            var post = await _forumService.UpdatePostAsync(id, userId, request.Content.Trim(), request.DocumentUrl, request.SharedLink);
            
            if (post == null)
                return NotFound(new { success = false, message = "Bài viết không tồn tại hoặc bạn không có quyền sửa" });

            return Ok(new { success = true, data = post, message = "Cập nhật bài viết thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating forum post {PostId}", id);
            return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật bài viết" });
        }
    }

    /// <summary>
    /// Delete a forum post
    /// </summary>
    [HttpDelete("posts/{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });

            var result = await _forumService.DeletePostAsync(id, userId);
            
            if (!result)
                return NotFound(new { success = false, message = "Bài viết không tồn tại hoặc bạn không có quyền xóa" });

            return Ok(new { success = true, message = "Xóa bài viết thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting forum post {PostId}", id);
            return StatusCode(500, new { success = false, message = "Lỗi khi xóa bài viết" });
        }
    }

    /// <summary>
    /// Add a comment to a post
    /// </summary>
    [HttpPost("posts/{postId}/comments")]
    public async Task<IActionResult> AddComment(int postId, [FromBody] CreateCommentRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });

            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { success = false, message = "Nội dung bình luận không được trống" });

            var comment = await _forumService.AddCommentAsync(postId, userId, request.Content.Trim());
            
            if (comment == null)
                return NotFound(new { success = false, message = "Bài viết không tồn tại" });

            return Ok(new { success = true, data = comment, message = "Bình luận thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to post {PostId}", postId);
            return StatusCode(500, new { success = false, message = "Lỗi khi bình luận" });
        }
    }

    /// <summary>
    /// Get comments for a post
    /// </summary>
    [HttpGet("posts/{postId}/comments")]
    public async Task<IActionResult> GetComments(int postId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var comments = await _forumService.GetCommentsAsync(postId, page, pageSize);
            return Ok(new { success = true, data = comments });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for post {PostId}", postId);
            return StatusCode(500, new { success = false, message = "Lỗi khi lấy bình luận" });
        }
    }

    /// <summary>
    /// Delete a comment
    /// </summary>
    [HttpDelete("comments/{id}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });

            var result = await _forumService.DeleteCommentAsync(id, userId);
            
            if (!result)
                return NotFound(new { success = false, message = "Bình luận không tồn tại" });

            return Ok(new { success = true, message = "Xóa bình luận thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", id);
            return StatusCode(500, new { success = false, message = "Lỗi khi xóa bình luận" });
        }
    }

    /// <summary>
    /// Toggle like on a post
    /// </summary>
    [HttpPost("posts/{id}/like")]
    public async Task<IActionResult> ToggleLike(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });

            var isLiked = await _forumService.ToggleLikeAsync(id, userId);
            
            return Ok(new 
            { 
                success = true, 
                data = new { isLiked },
                message = isLiked ? "Đã thích bài viết" : "Đã bỏ thích bài viết"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling like on post {PostId}", id);
            return StatusCode(500, new { success = false, message = "Lỗi khi thích bài viết" });
        }
    }
}

public class CreatePostRequest
{
    public string Content { get; set; } = string.Empty;
    public string? DocumentUrl { get; set; }
    public string? SharedLink { get; set; }
}

public class UpdatePostRequest
{
    public string Content { get; set; } = string.Empty;
    public string? DocumentUrl { get; set; }
    public string? SharedLink { get; set; }
}

public class CreateCommentRequest
{
    public string Content { get; set; } = string.Empty;
}

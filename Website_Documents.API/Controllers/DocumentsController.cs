using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly ISharedDocumentService _documentService;
    private readonly IStudySpaceService _studySpaceService;
    private readonly IStorageService _storageService;

    public DocumentsController(
        ISharedDocumentService documentService,
        IStudySpaceService studySpaceService,
        IStorageService storageService)
    {
        _documentService = documentService;
        _studySpaceService = studySpaceService;
        _storageService = storageService;
    }

    private long? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    private string? GetUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value 
            ?? User.FindFirst("name")?.Value
            ?? User.FindFirst("fullName")?.Value;
    }

    private string? GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value 
            ?? User.FindFirst("role")?.Value;
    }

    private bool IsAdmin()
    {
        var role = GetUserRole();
        return !string.IsNullOrEmpty(role) && role.ToLower() == "admin";
    }

    /// <summary>
    /// Lấy danh sách tài liệu với bộ lọc (chỉ hiển thị tài liệu đã duyệt)
    /// Admin có thể xem cả tài liệu chưa duyệt với tham số showAll=true
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDocuments(
        [FromQuery] int? subjectId,
        [FromQuery] int? topicId,
        [FromQuery] int? minQuestionCount,
        [FromQuery] int? maxQuestionCount,
        [FromQuery] int? gradeLevel,
        [FromQuery] string? documentType,
        [FromQuery] string? keyword,
        [FromQuery] string? sortBy = "created_at",
        [FromQuery] string? sortOrder = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Check if user is admin - admins can see all documents including pending
        var userRole = GetUserRole();
        var isAdmin = !string.IsNullOrEmpty(userRole) && userRole.ToLower() == "admin";
        
        // Check if user wants to see all documents (admin only)
        var showAll = Request.Query["showAll"].ToString().ToLower() == "true";
        
        var filter = new DocumentFilterRequest
        {
            SubjectId = subjectId,
            TopicId = topicId,
            MinQuestionCount = minQuestionCount,
            MaxQuestionCount = maxQuestionCount,
            GradeLevel = gradeLevel,
            DocumentType = documentType,
            Keyword = keyword,
            SortBy = sortBy,
            SortOrder = sortOrder,
            Page = page,
            PageSize = pageSize,
            // Admins see all documents including pending; regular users only see approved
            IncludeUnapproved = (isAdmin && showAll)
        };

        // Only show approved documents to regular users
        var documents = await _documentService.GetFilteredDocumentsAsync(filter);
        var totalCount = await _documentService.GetTotalCountAsync(filter);

        return Ok(new
        {
            success = true,
            data = documents,
            pagination = new
            {
                currentPage = page,
                pageSize,
                totalItems = totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            },
            isAdmin = isAdmin
        });
    }

    /// <summary>
    /// Lấy tất cả tài liệu bao gồm cả chưa duyệt (Admin only)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllDocuments(
        [FromQuery] int? subjectId,
        [FromQuery] int? topicId,
        [FromQuery] int? minQuestionCount,
        [FromQuery] int? maxQuestionCount,
        [FromQuery] int? gradeLevel,
        [FromQuery] string? documentType,
        [FromQuery] string? keyword,
        [FromQuery] string? moderationStatus,
        [FromQuery] string? sortBy = "created_at",
        [FromQuery] string? sortOrder = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new DocumentFilterRequest
        {
            SubjectId = subjectId,
            TopicId = topicId,
            MinQuestionCount = minQuestionCount,
            MaxQuestionCount = maxQuestionCount,
            GradeLevel = gradeLevel,
            DocumentType = documentType,
            Keyword = keyword,
            ModerationStatus = moderationStatus,
            IncludeUnapproved = true,
            SortBy = sortBy,
            SortOrder = sortOrder,
            Page = page,
            PageSize = pageSize
        };

        var documents = await _documentService.GetFilteredDocumentsAsync(filter);
        var totalCount = await _documentService.GetTotalCountAsync(filter);

        return Ok(new
        {
            success = true,
            data = documents,
            pagination = new
            {
                currentPage = page,
                pageSize,
                totalItems = totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        });
    }

    /// <summary>
    /// Lấy chi tiết một tài liệu
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(int id)
    {
        var document = await _documentService.GetDocumentByIdAsync(id);
        if (document == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy tài liệu" });
        }

        // Increment view count
        await _documentService.IncrementViewCountAsync(id);

        return Ok(new { success = true, data = document });
    }

    /// <summary>
    /// Tạo tài liệu mới (yêu cầu đăng nhập - bất kỳ user nào cũng có thể upload)
    /// Tài liệu sẽ có trạng thái "pending" và chờ admin duyệt
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { success = false, message = "Tiêu đề không được để trống" });
        }

        if (request.DocumentType == "link" && string.IsNullOrWhiteSpace(request.LinkUrl))
        {
            return BadRequest(new { success = false, message = "Link URL không được để trống khi chia sẻ link" });
        }

        var userId = GetUserId();
        var userName = GetUserName();

        var document = await _documentService.CreateDocumentAsync(request, userId, userName);

        return Ok(new { 
            success = true, 
            message = "Tạo tài liệu thành công! Tài liệu đang chờ admin duyệt và sẽ hiển thị sau khi được phê duyệt.", 
            data = document 
        });
    }

    /// <summary>
    /// Cập nhật tài liệu (yêu cầu đăng nhập, chỉ chủ sở hữu)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateDocument(int id, [FromBody] UpdateDocumentRequest request)
    {
        var userId = GetUserId();
        var document = await _documentService.UpdateDocumentAsync(id, request, userId);

        if (document == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy tài liệu hoặc bạn không có quyền chỉnh sửa" });
        }

        return Ok(new { success = true, message = "Cập nhật thành công", data = document });
    }

    /// <summary>
    /// Xóa tài liệu (yêu cầu đăng nhập, chỉ chủ sở hữu)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var userId = GetUserId();
        var result = await _documentService.DeleteDocumentAsync(id, userId);

        if (!result)
        {
            return NotFound(new { success = false, message = "Không tìm thấy tài liệu hoặc bạn không có quyền xóa" });
        }

        return Ok(new { success = true, message = "Xóa tài liệu thành công" });
    }

    /// <summary>
    /// Tăng số lượt tải xuống
    /// </summary>
    [HttpPost("{id}/download")]
    public async Task<IActionResult> IncrementDownload(int id)
    {
        var result = await _documentService.IncrementDownloadCountAsync(id);
        if (!result)
        {
            return NotFound(new { success = false, message = "Không tìm thấy tài liệu" });
        }

        return Ok(new { success = true, message = "Đã cập nhật số lượt tải" });
    }

    /// <summary>
    /// Tăng số lượt thích
    /// </summary>
    [HttpPost("{id}/like")]
    public async Task<IActionResult> IncrementLike(int id)
    {
        var result = await _documentService.IncrementLikeCountAsync(id);
        if (!result)
        {
            return NotFound(new { success = false, message = "Không tìm thấy tài liệu" });
        }

        return Ok(new { success = true, message = "Đã thích tài liệu" });
    }

    /// <summary>
    /// Lấy tài liệu theo môn học
    /// </summary>
    [HttpGet("by-subject/{subjectId}")]
    public async Task<IActionResult> GetBySubject(
        int subjectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var documents = await _documentService.GetDocumentsBySubjectAsync(subjectId, page, pageSize);
        return Ok(new { success = true, data = documents });
    }

    /// <summary>
    /// Lấy tài liệu theo chủ đề
    /// </summary>
    [HttpGet("by-topic/{topicId}")]
    public async Task<IActionResult> GetByTopic(
        int topicId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var documents = await _documentService.GetDocumentsByTopicAsync(topicId, page, pageSize);
        return Ok(new { success = true, data = documents });
    }

    /// <summary>
    /// Lấy tài liệu theo số lượng câu hỏi
    /// </summary>
    [HttpGet("by-question-count")]
    public async Task<IActionResult> GetByQuestionCount(
        [FromQuery] int? min,
        [FromQuery] int? max,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var documents = await _documentService.GetDocumentsByQuestionCountRangeAsync(min, max, page, pageSize);
        return Ok(new { success = true, data = documents });
    }

    /// <summary>
    /// Tìm kiếm tài liệu
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(new { success = false, message = "Từ khóa tìm kiếm không được để trống" });
        }

        var documents = await _documentService.SearchDocumentsAsync(keyword, page, pageSize);
        return Ok(new { success = true, data = documents });
    }

    /// <summary>
    /// Lấy tài liệu của người dùng hiện tại (bao gồm cả đang chờ duyệt)
    /// </summary>
    [HttpGet("my-documents")]
    [Authorize]
    public async Task<IActionResult> GetMyDocuments([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new { success = false, message = "Vui lòng đăng nhập" });
        }

        var filter = new DocumentFilterRequest
        {
            SortBy = "created_at",
            SortOrder = "desc",
            Page = page,
            PageSize = pageSize
        };

        // Get all documents (including pending) and filter by user
        filter.IncludeUnapproved = true;
        
        var allDocuments = await _documentService.GetFilteredDocumentsAsync(filter);
        var myDocuments = allDocuments.Where(d => d.SharedByUserId == userId.Value).ToList();

        return Ok(new { success = true, data = myDocuments });
    }

    /// <summary>
    /// Upload file lên storage (Neon Storage)
    /// </summary>
    [HttpPost("upload")]
    [Authorize]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string? folder = "documents")
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "Không có file nào được chọn" });
        }

        // Validate file size (max 100MB)
        if (file.Length > 100 * 1024 * 1024)
        {
            return BadRequest(new { success = false, message = "File quá lớn. Kích thước tối đa là 100MB" });
        }

        // Validate file type
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".png", ".jpg", ".jpeg", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { success = false, message = "Định dạng file không được hỗ trợ" });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var fileUrl = await _storageService.UploadFileAsync(stream, file.FileName, file.ContentType, folder);

            return Ok(new
            {
                success = true,
                message = "Upload file thành công",
                data = new
                {
                    fileUrl,
                    fileName = file.FileName,
                    fileType = file.ContentType,
                    fileSize = file.Length
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { success = false, message = "Storage service chưa được cấu hình: " + ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Lỗi khi upload file: " + ex.Message });
        }
    }

    /// <summary>
    /// Xóa file khỏi storage
    /// </summary>
    [HttpDelete("files")]
    [Authorize]
    public async Task<IActionResult> DeleteFile([FromBody] DeleteFileRequest request)
    {
        if (string.IsNullOrEmpty(request.FileUrl))
        {
            return BadRequest(new { success = false, message = "URL file không hợp lệ" });
        }

        try
        {
            var result = await _storageService.DeleteFileAsync(request.FileUrl);
            if (result)
            {
                return Ok(new { success = true, message = "Xóa file thành công" });
            }
            return BadRequest(new { success = false, message = "Không thể xóa file" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Lỗi khi xóa file: " + ex.Message });
        }
    }
}

public class DeleteFileRequest
{
    public string FileUrl { get; set; } = string.Empty;
}

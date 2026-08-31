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
[Authorize(Roles = "admin")]
public class ModerationController : ControllerBase
{
    private readonly ISharedDocumentService _documentService;

    public ModerationController(ISharedDocumentService documentService)
    {
        _documentService = documentService;
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

    /// <summary>
    /// Lấy danh sách tài liệu chờ kiểm duyệt
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingDocuments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var documents = await _documentService.GetPendingDocumentsAsync(page, pageSize);
        var totalCount = await _documentService.GetPendingCountAsync();

        return Ok(new
        {
            success = true,
            data = documents,
            pendingCount = totalCount,
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
    /// Lấy số lượng tài liệu chờ kiểm duyệt
    /// </summary>
    [HttpGet("pending/count")]
    public async Task<IActionResult> GetPendingCount()
    {
        var count = await _documentService.GetPendingCountAsync();
        return Ok(new { success = true, count });
    }

    /// <summary>
    /// Lấy tài liệu theo trạng thái kiểm duyệt
    /// </summary>
    [HttpGet("documents")]
    public async Task<IActionResult> GetDocumentsByStatus(
        [FromQuery] string? status, // pending, approved, rejected
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new DocumentFilterRequest
        {
            ModerationStatus = status,
            IncludeUnapproved = true,
            SortBy = "created_at",
            SortOrder = "desc",
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
    /// Phê duyệt tài liệu
    /// </summary>
    [HttpPost("approve/{id}")]
    public async Task<IActionResult> ApproveDocument(
        int id,
        [FromBody] ModerationActionRequest? request)
    {
        var moderatorId = GetUserId();
        var moderatorName = GetUserName();

        var document = await _documentService.ApproveDocumentAsync(id, moderatorId, moderatorName, request?.Notes);

        if (document == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy tài liệu" });
        }

        return Ok(new { success = true, message = "Đã phê duyệt tài liệu", data = document });
    }

    /// <summary>
    /// Từ chối tài liệu
    /// </summary>
    [HttpPost("reject/{id}")]
    public async Task<IActionResult> RejectDocument(
        int id,
        [FromBody] RejectDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(new { success = false, message = "Lý do từ chối không được để trống" });
        }

        var moderatorId = GetUserId();
        var moderatorName = GetUserName();

        var document = await _documentService.RejectDocumentAsync(id, moderatorId, moderatorName, request.Reason);

        if (document == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy tài liệu" });
        }

        return Ok(new { success = true, message = "Đã từ chối tài liệu", data = document });
    }

    /// <summary>
    /// Cập nhật nội dung tài liệu (chỉnh sửa trước khi duyệt)
    /// </summary>
    [HttpPut("documents/{id}")]
    public async Task<IActionResult> UpdateDocument(
        int id,
        [FromBody] UpdateDocumentRequest request)
    {
        var moderatorId = GetUserId();
        var moderatorName = GetUserName();

        var document = await _documentService.UpdateDocumentContentAsync(id, request, moderatorId, moderatorName);

        if (document == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy tài liệu" });
        }

        return Ok(new { success = true, message = "Đã cập nhật tài liệu", data = document });
    }

    /// <summary>
    /// Xóa tài liệu (admin only)
    /// </summary>
    [HttpDelete("documents/{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var moderatorId = GetUserId();
        var result = await _documentService.DeleteDocumentAsync(id, moderatorId);

        if (!result)
        {
            return NotFound(new { success = false, message = "Không tìm thấy tài liệu" });
        }

        return Ok(new { success = true, message = "Đã xóa tài liệu" });
    }

    /// <summary>
    /// Lấy chi tiết tài liệu (admin xem cả tài liệu chưa duyệt)
    /// </summary>
    [HttpGet("documents/{id}")]
    public async Task<IActionResult> GetDocument(int id)
    {
        var document = await _documentService.GetDocumentByIdAsync(id);
        if (document == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy tài liệu" });
        }

        return Ok(new { success = true, data = document });
    }

    /// <summary>
    /// Phê duyệt nhiều tài liệu cùng lúc
    /// </summary>
    [HttpPost("approve-batch")]
    public async Task<IActionResult> ApproveBatch([FromBody] BatchModerationRequest request)
    {
        if (request.DocumentIds == null || request.DocumentIds.Length == 0)
        {
            return BadRequest(new { success = false, message = "Danh sách ID tài liệu trống" });
        }

        var moderatorId = GetUserId();
        var moderatorName = GetUserName();
        var results = new List<object>();
        var errors = new List<string>();

        foreach (var docId in request.DocumentIds)
        {
            try
            {
                var document = await _documentService.ApproveDocumentAsync(docId, moderatorId, moderatorName, request.Notes);
                if (document != null)
                {
                    results.Add(new { id = docId, success = true, title = document.Title });
                }
                else
                {
                    errors.Add($"ID {docId}: Không tìm thấy tài liệu");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"ID {docId}: {ex.Message}");
            }
        }

        return Ok(new
        {
            success = true,
            message = $"Đã phê duyệt {results.Count} tài liệu",
            approved = results,
            errors = errors
        });
    }

    /// <summary>
    /// Từ chối nhiều tài liệu cùng lúc
    /// </summary>
    [HttpPost("reject-batch")]
    public async Task<IActionResult> RejectBatch([FromBody] BatchRejectRequest request)
    {
        if (request.DocumentIds == null || request.DocumentIds.Length == 0)
        {
            return BadRequest(new { success = false, message = "Danh sách ID tài liệu trống" });
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(new { success = false, message = "Lý do từ chối không được để trống" });
        }

        var moderatorId = GetUserId();
        var moderatorName = GetUserName();
        var results = new List<object>();
        var errors = new List<string>();

        foreach (var docId in request.DocumentIds)
        {
            try
            {
                var document = await _documentService.RejectDocumentAsync(docId, moderatorId, moderatorName, request.Reason);
                if (document != null)
                {
                    results.Add(new { id = docId, success = true, title = document.Title });
                }
                else
                {
                    errors.Add($"ID {docId}: Không tìm thấy tài liệu");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"ID {docId}: {ex.Message}");
            }
        }

        return Ok(new
        {
            success = true,
            message = $"Đã từ chối {results.Count} tài liệu",
            rejected = results,
            errors = errors
        });
    }
}

public class ModerationActionRequest
{
    public string? Notes { get; set; }
}

public class RejectDocumentRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class BatchModerationRequest
{
    public int[] DocumentIds { get; set; } = Array.Empty<int>();
    public string? Notes { get; set; }
}

public class BatchRejectRequest
{
    public int[] DocumentIds { get; set; } = Array.Empty<int>();
    public string Reason { get; set; } = string.Empty;
}

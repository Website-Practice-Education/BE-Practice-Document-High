using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class SharedDocumentService : ISharedDocumentService
{
    private readonly BookstoreDbContext _context;

    public SharedDocumentService(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<SharedDocument> CreateDocumentAsync(CreateDocumentRequest request, long? userId, string? userName)
    {
        var document = new SharedDocument
        {
            Title = request.Title,
            Description = request.Description,
            DocumentType = request.DocumentType,
            FileUrl = request.FileUrl,
            FileType = request.FileType,
            FileSize = request.FileSize,
            SubjectId = request.SubjectId,
            TopicId = request.TopicId,
            QuestionCount = request.QuestionCount,
            GradeLevel = request.GradeLevel,
            LinkUrl = request.LinkUrl,
            LinkSource = request.LinkSource,
            SharedByUserId = userId.HasValue ? (int)userId.Value : null,
            SharedByName = userName,
            ViewCount = 0,
            DownloadCount = 0,
            LikeCount = 0,
            IsActive = true,
            IsVerified = false,
            ModerationStatus = "pending", // Default to pending
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.SharedDocuments.Add(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<SharedDocument?> GetDocumentByIdAsync(int id)
    {
        return await _context.SharedDocuments
            .Include(d => d.Subject)
            .Include(d => d.Topic)
            .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);
    }

    public async Task<List<SharedDocument>> GetAllDocumentsAsync(int page = 1, int pageSize = 20)
    {
        return await _context.SharedDocuments
            .Include(d => d.Subject)
            .Include(d => d.Topic)
            .Where(d => d.IsActive && d.ModerationStatus == "approved")
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<SharedDocument>> GetDocumentsBySubjectAsync(int subjectId, int page = 1, int pageSize = 20)
    {
        return await _context.SharedDocuments
            .Include(d => d.Subject)
            .Include(d => d.Topic)
            .Where(d => d.SubjectId == subjectId && d.IsActive && d.ModerationStatus == "approved")
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<SharedDocument>> GetDocumentsByTopicAsync(int topicId, int page = 1, int pageSize = 20)
    {
        return await _context.SharedDocuments
            .Include(d => d.Subject)
            .Include(d => d.Topic)
            .Where(d => d.TopicId == topicId && d.IsActive && d.ModerationStatus == "approved")
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<SharedDocument>> GetDocumentsByQuestionCountRangeAsync(int? minQuestions, int? maxQuestions, int page = 1, int pageSize = 20)
    {
        var query = _context.SharedDocuments
            .Include(d => d.Subject)
            .Include(d => d.Topic)
            .Where(d => d.IsActive && d.ModerationStatus == "approved");

        if (minQuestions.HasValue)
        {
            query = query.Where(d => d.QuestionCount >= minQuestions.Value);
        }

        if (maxQuestions.HasValue)
        {
            query = query.Where(d => d.QuestionCount <= maxQuestions.Value);
        }

        return await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<SharedDocument>> SearchDocumentsAsync(string keyword, int page = 1, int pageSize = 20)
    {
        var lowerKeyword = keyword.ToLower();
        return await _context.SharedDocuments
            .Include(d => d.Subject)
            .Include(d => d.Topic)
            .Where(d => d.IsActive && d.ModerationStatus == "approved" && (
                d.Title.ToLower().Contains(lowerKeyword) ||
                (d.Description != null && d.Description.ToLower().Contains(lowerKeyword)) ||
                (d.SharedByName != null && d.SharedByName.ToLower().Contains(lowerKeyword))
            ))
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<SharedDocument?> UpdateDocumentAsync(int id, UpdateDocumentRequest request, long? userId)
    {
        var document = await _context.SharedDocuments.FindAsync(id);
        if (document == null || !document.IsActive)
        {
            return null;
        }

        // Check if user owns the document or is admin
        if (userId.HasValue && document.SharedByUserId != userId.Value)
        {
            // For now, allow update - can add admin check later
        }

        if (request.Title != null)
            document.Title = request.Title;
        if (request.Description != null)
            document.Description = request.Description;
        if (request.SubjectId.HasValue)
            document.SubjectId = request.SubjectId;
        if (request.TopicId.HasValue)
            document.TopicId = request.TopicId;
        if (request.QuestionCount.HasValue)
            document.QuestionCount = request.QuestionCount;
        if (request.GradeLevel.HasValue)
            document.GradeLevel = request.GradeLevel;
        if (request.LinkUrl != null)
            document.LinkUrl = request.LinkUrl;
        if (request.LinkSource != null)
            document.LinkSource = request.LinkSource;
        if (request.IsVerified.HasValue)
            document.IsVerified = request.IsVerified.Value;
        if (request.IsActive.HasValue)
            document.IsActive = request.IsActive.Value;

        document.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<bool> DeleteDocumentAsync(int id, long? userId)
    {
        var document = await _context.SharedDocuments.FindAsync(id);
        if (document == null || !document.IsActive)
        {
            return false;
        }

        // Check ownership
        if (userId.HasValue && document.SharedByUserId != userId.Value)
        {
            return false;
        }

        document.IsActive = false;
        document.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IncrementViewCountAsync(int id)
    {
        var document = await _context.SharedDocuments.FindAsync(id);
        if (document == null)
        {
            return false;
        }

        document.ViewCount++;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IncrementDownloadCountAsync(int id)
    {
        var document = await _context.SharedDocuments.FindAsync(id);
        if (document == null)
        {
            return false;
        }

        document.DownloadCount++;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IncrementLikeCountAsync(int id)
    {
        var document = await _context.SharedDocuments.FindAsync(id);
        if (document == null)
        {
            return false;
        }

        document.LikeCount++;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SharedDocument>> GetFilteredDocumentsAsync(DocumentFilterRequest filter)
    {
        var query = _context.SharedDocuments
            .Include(d => d.Subject)
            .Include(d => d.Topic)
            .Where(d => d.IsActive);

        // Filter by moderation status - only show approved unless specified
        if (!filter.IncludeUnapproved.HasValue || !filter.IncludeUnapproved.Value)
        {
            query = query.Where(d => d.ModerationStatus == "approved");
        }
        else if (!string.IsNullOrEmpty(filter.ModerationStatus))
        {
            // Include both matching status and NULL (treat NULL as pending)
            if (filter.ModerationStatus == "pending")
            {
                query = query.Where(d => d.ModerationStatus == "pending" || d.ModerationStatus == null);
            }
            else
            {
                query = query.Where(d => d.ModerationStatus == filter.ModerationStatus);
            }
        }

        // Apply filters
        if (filter.SubjectId.HasValue)
        {
            query = query.Where(d => d.SubjectId == filter.SubjectId.Value);
        }

        if (filter.TopicId.HasValue)
        {
            query = query.Where(d => d.TopicId == filter.TopicId.Value);
        }

        if (filter.MinQuestionCount.HasValue)
        {
            query = query.Where(d => d.QuestionCount >= filter.MinQuestionCount.Value);
        }

        if (filter.MaxQuestionCount.HasValue)
        {
            query = query.Where(d => d.QuestionCount <= filter.MaxQuestionCount.Value);
        }

        if (filter.GradeLevel.HasValue)
        {
            query = query.Where(d => d.GradeLevel == filter.GradeLevel.Value);
        }

        if (!string.IsNullOrEmpty(filter.DocumentType))
        {
            query = query.Where(d => d.DocumentType == filter.DocumentType);
        }

        if (!string.IsNullOrEmpty(filter.Keyword))
        {
            var lowerKeyword = filter.Keyword.ToLower();
            query = query.Where(d =>
                d.Title.ToLower().Contains(lowerKeyword) ||
                (d.Description != null && d.Description.ToLower().Contains(lowerKeyword)));
        }

        // Apply sorting
        query = filter.SortBy?.ToLower() switch
        {
            "view_count" => filter.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(d => d.ViewCount)
                : query.OrderByDescending(d => d.ViewCount),
            "like_count" => filter.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(d => d.LikeCount)
                : query.OrderByDescending(d => d.LikeCount),
            "download_count" => filter.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(d => d.DownloadCount)
                : query.OrderByDescending(d => d.DownloadCount),
            "question_count" => filter.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(d => d.QuestionCount)
                : query.OrderByDescending(d => d.QuestionCount),
            _ => filter.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(d => d.CreatedAt)
                : query.OrderByDescending(d => d.CreatedAt)
        };

        return await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(DocumentFilterRequest? filter = null)
    {
        if (filter == null)
        {
            return await _context.SharedDocuments.CountAsync(d => d.IsActive && d.ModerationStatus == "approved");
        }

        var query = _context.SharedDocuments.Where(d => d.IsActive);

        if (!filter.IncludeUnapproved.HasValue || !filter.IncludeUnapproved.Value)
        {
            query = query.Where(d => d.ModerationStatus == "approved");
        }
        else if (!string.IsNullOrEmpty(filter.ModerationStatus))
        {
            // Include both matching status and NULL (treat NULL as pending)
            if (filter.ModerationStatus == "pending")
            {
                query = query.Where(d => d.ModerationStatus == "pending" || d.ModerationStatus == null);
            }
            else
            {
                query = query.Where(d => d.ModerationStatus == filter.ModerationStatus);
            }
        }

        if (filter.SubjectId.HasValue)
            query = query.Where(d => d.SubjectId == filter.SubjectId.Value);
        if (filter.TopicId.HasValue)
            query = query.Where(d => d.TopicId == filter.TopicId.Value);
        if (filter.MinQuestionCount.HasValue)
            query = query.Where(d => d.QuestionCount >= filter.MinQuestionCount.Value);
        if (filter.MaxQuestionCount.HasValue)
            query = query.Where(d => d.QuestionCount <= filter.MaxQuestionCount.Value);
        if (filter.GradeLevel.HasValue)
            query = query.Where(d => d.GradeLevel == filter.GradeLevel.Value);
        if (!string.IsNullOrEmpty(filter.DocumentType))
            query = query.Where(d => d.DocumentType == filter.DocumentType);
        if (!string.IsNullOrEmpty(filter.Keyword))
        {
            var lowerKeyword = filter.Keyword.ToLower();
            query = query.Where(d =>
                d.Title.ToLower().Contains(lowerKeyword) ||
                (d.Description != null && d.Description.ToLower().Contains(lowerKeyword)));
        }

        return await query.CountAsync();
    }

    // ===== MODERATION METHODS =====

    public async Task<List<SharedDocument>> GetPendingDocumentsAsync(int page = 1, int pageSize = 20)
    {
        return await _context.SharedDocuments
            .Include(d => d.Subject)
            .Include(d => d.Topic)
            .Where(d => d.IsActive && (d.ModerationStatus == "pending" || d.ModerationStatus == null))
            .OrderBy(d => d.CreatedAt) // Oldest first (FIFO)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await _context.SharedDocuments
            .CountAsync(d => d.IsActive && (d.ModerationStatus == "pending" || d.ModerationStatus == null));
    }

    public async Task<SharedDocument?> ApproveDocumentAsync(int id, long? moderatorId, string? moderatorName, string? notes = null)
    {
        var document = await _context.SharedDocuments.FindAsync(id);
        if (document == null || !document.IsActive)
        {
            return null;
        }

        document.ModerationStatus = "approved";
        document.IsVerified = true;
        document.ModeratedByUserId = moderatorId.HasValue ? (int)moderatorId.Value : null;
        document.ModeratedByName = moderatorName;
        document.ModeratedAt = DateTime.UtcNow;
        
        if (!string.IsNullOrEmpty(notes))
        {
            document.ModerationNotes = notes;
        }

        document.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<SharedDocument?> RejectDocumentAsync(int id, long? moderatorId, string? moderatorName, string? reason)
    {
        var document = await _context.SharedDocuments.FindAsync(id);
        if (document == null || !document.IsActive)
        {
            return null;
        }

        document.ModerationStatus = "rejected";
        document.IsVerified = false;
        document.ModeratedByUserId = moderatorId.HasValue ? (int)moderatorId.Value : null;
        document.ModeratedByName = moderatorName;
        document.ModeratedAt = DateTime.UtcNow;
        document.ModerationNotes = reason ?? "Tài liệu không đạt yêu cầu";

        document.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<SharedDocument?> UpdateDocumentContentAsync(int id, UpdateDocumentRequest request, long? moderatorId, string? moderatorName)
    {
        var document = await _context.SharedDocuments.FindAsync(id);
        if (document == null || !document.IsActive)
        {
            return null;
        }

        // Update content fields
        if (request.Title != null)
            document.Title = request.Title;
        if (request.Description != null)
            document.Description = request.Description;
        if (request.SubjectId.HasValue)
            document.SubjectId = request.SubjectId;
        if (request.TopicId.HasValue)
            document.TopicId = request.TopicId;
        if (request.QuestionCount.HasValue)
            document.QuestionCount = request.QuestionCount;
        if (request.GradeLevel.HasValue)
            document.GradeLevel = request.GradeLevel;
        if (request.LinkUrl != null)
            document.LinkUrl = request.LinkUrl;
        if (request.LinkSource != null)
            document.LinkSource = request.LinkSource;

        // Note: Moderation status remains unchanged unless explicitly set
        // Moderator info for audit trail
        document.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return document;
    }
}

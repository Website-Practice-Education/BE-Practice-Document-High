using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface ISharedDocumentService
{
    // CRUD operations
    Task<SharedDocument> CreateDocumentAsync(CreateDocumentRequest request, long? userId, string? userName);
    Task<SharedDocument?> GetDocumentByIdAsync(int id);
    Task<List<SharedDocument>> GetAllDocumentsAsync(int page = 1, int pageSize = 20);
    Task<List<SharedDocument>> GetDocumentsBySubjectAsync(int subjectId, int page = 1, int pageSize = 20);
    Task<List<SharedDocument>> GetDocumentsByTopicAsync(int topicId, int page = 1, int pageSize = 20);
    Task<List<SharedDocument>> GetDocumentsByQuestionCountRangeAsync(int? minQuestions, int? maxQuestions, int page = 1, int pageSize = 20);
    Task<List<SharedDocument>> SearchDocumentsAsync(string keyword, int page = 1, int pageSize = 20);
    Task<SharedDocument?> UpdateDocumentAsync(int id, UpdateDocumentRequest request, long? userId);
    Task<bool> DeleteDocumentAsync(int id, long? userId);
    
    // Statistics
    Task<bool> IncrementViewCountAsync(int id);
    Task<bool> IncrementDownloadCountAsync(int id);
    Task<bool> IncrementLikeCountAsync(int id);
    
    // Filtering
    Task<List<SharedDocument>> GetFilteredDocumentsAsync(DocumentFilterRequest filter);
    Task<int> GetTotalCountAsync(DocumentFilterRequest? filter = null);

    // ===== MODERATION METHODS =====
    Task<List<SharedDocument>> GetPendingDocumentsAsync(int page = 1, int pageSize = 20);
    Task<int> GetPendingCountAsync();
    Task<SharedDocument?> ApproveDocumentAsync(int id, long? moderatorId, string? moderatorName, string? notes = null);
    Task<SharedDocument?> RejectDocumentAsync(int id, long? moderatorId, string? moderatorName, string? reason);
    Task<SharedDocument?> UpdateDocumentContentAsync(int id, UpdateDocumentRequest request, long? moderatorId, string? moderatorName);
}

public class CreateDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DocumentType { get; set; } = "link"; // "file" or "link"
    public string? FileUrl { get; set; }
    public string? FileType { get; set; }
    public long? FileSize { get; set; }
    public int? SubjectId { get; set; }
    public int? TopicId { get; set; }
    public int? QuestionCount { get; set; }
    public int? GradeLevel { get; set; }
    public string? LinkUrl { get; set; }
    public string? LinkSource { get; set; }
}

public class UpdateDocumentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? SubjectId { get; set; }
    public int? TopicId { get; set; }
    public int? QuestionCount { get; set; }
    public int? GradeLevel { get; set; }
    public string? LinkUrl { get; set; }
    public string? LinkSource { get; set; }
    public bool? IsVerified { get; set; }
    public bool? IsActive { get; set; }
}

public class DocumentFilterRequest
{
    public int? SubjectId { get; set; }
    public int? TopicId { get; set; }
    public int? MinQuestionCount { get; set; }
    public int? MaxQuestionCount { get; set; }
    public int? GradeLevel { get; set; }
    public string? DocumentType { get; set; }
    public string? Keyword { get; set; }
    public string? ModerationStatus { get; set; } // Filter by moderation status
    public bool? IncludeUnapproved { get; set; } // Include documents not yet approved
    public string? SortBy { get; set; } = "created_at";
    public string? SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

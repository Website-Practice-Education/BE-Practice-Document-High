using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("shared_documents")]
public class SharedDocument
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Required]
    [Column("document_type")]
    [MaxLength(50)]
    public string DocumentType { get; set; } = "link"; // "file" or "link"

    [Column("file_url")]
    [MaxLength(1000)]
    public string? FileUrl { get; set; }

    [Column("file_type")]
    [MaxLength(50)]
    public string? FileType { get; set; }

    [Column("file_size")]
    public long? FileSize { get; set; }

    // Classification fields
    [Column("subject_id")]
    public int? SubjectId { get; set; }

    [Column("topic_id")]
    public int? TopicId { get; set; }

    [Column("question_count")]
    public int? QuestionCount { get; set; } // Số lượng câu hỏi

    [Column("grade_level")]
    public int? GradeLevel { get; set; } // Lớp học

    // Link metadata
    [Column("link_url")]
    [MaxLength(1000)]
    public string? LinkUrl { get; set; }

    [Column("link_source")]
    [MaxLength(100)]
    public string? LinkSource { get; set; } // Nguồn: Google Drive, Facebook,...

    // User info
    [Column("shared_by_user_id")]
    public long? SharedByUserId { get; set; }

    [Column("shared_by_name")]
    [MaxLength(255)]
    public string? SharedByName { get; set; }

    // Statistics
    [Column("view_count")]
    public int ViewCount { get; set; } = 0;

    [Column("download_count")]
    public int DownloadCount { get; set; } = 0;

    [Column("like_count")]
    public int LikeCount { get; set; } = 0;

    // Status
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("is_verified")]
    public bool IsVerified { get; set; } = false;

    // ===== MODERATION FIELDS =====
    // Moderation status: "pending", "approved", "rejected"
    [Column("moderation_status")]
    [MaxLength(20)]
    public string ModerationStatus { get; set; } = "pending";

    // Notes from moderator (reason for rejection, feedback, etc.)
    [Column("moderation_notes", TypeName = "text")]
    public string? ModerationNotes { get; set; }

    // Who moderated this document
    [Column("moderated_by_user_id")]
    public long? ModeratedByUserId { get; set; }

    [Column("moderated_by_name")]
    [MaxLength(255)]
    public string? ModeratedByName { get; set; }

    // When moderation happened
    [Column("moderated_at")]
    public DateTime? ModeratedAt { get; set; }

    // Timestamps
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SubjectId")]
    public virtual Subject? Subject { get; set; }

    [ForeignKey("TopicId")]
    public virtual Topic? Topic { get; set; }

    [ForeignKey("SharedByUserId")]
    public virtual User? SharedByUser { get; set; }

    [ForeignKey("ModeratedByUserId")]
    public virtual User? ModeratedByUser { get; set; }
}

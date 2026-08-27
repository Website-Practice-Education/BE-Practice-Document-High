using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[Table("user_attempts")]
[Index("ExamId", Name = "idx_user_attempts_exam")]
[Index("Status", Name = "idx_user_attempts_status")]
[Index("UserId", Name = "idx_user_attempts_user")]
public partial class UserAttempt
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("exam_id")]
    public long? ExamId { get; set; }

    [Column("subject_id")]
    public int? SubjectId { get; set; }

    [Column("topic_id")]
    public int? TopicId { get; set; }

    [Column("started_at")]
    public DateTime? StartedAt { get; set; }

    [Column("finished_at")]
    public DateTime? FinishedAt { get; set; }

    [Column("time_limit_seconds")]
    public int? TimeLimitSeconds { get; set; }

    [Column("remaining_seconds")]
    public int? RemainingSeconds { get; set; }

    [Column("time_spent_seconds")]
    public int? TimeSpentSeconds { get; set; }

    [Column("is_timeout")]
    public bool? IsTimeout { get; set; }

    [Column("submitted_by")]
    [StringLength(20)]
    public string? SubmittedBy { get; set; }

    [Column("score")]
    [Precision(6, 2)]
    public decimal? Score { get; set; }

    [Column("max_score")]
    [Precision(6, 2)]
    public decimal? MaxScore { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string? Status { get; set; }

    [ForeignKey("ExamId")]
    [InverseProperty("UserAttempts")]
    public virtual Exam? Exam { get; set; }

    [ForeignKey("SubjectId")]
    [InverseProperty("UserAttempts")]
    public virtual Subject? Subject { get; set; }

    [ForeignKey("TopicId")]
    [InverseProperty("UserAttempts")]
    public virtual Topic? Topic { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserAttempts")]
    public virtual User User { get; set; } = null!;

    [InverseProperty("Attempt")]
    public virtual ICollection<UserAnswerHistory> UserAnswerHistories { get; set; } = new List<UserAnswerHistory>();

    [InverseProperty("Attempt")]
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}

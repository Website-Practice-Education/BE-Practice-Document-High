using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[Table("exams")]
public partial class Exam
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("subject_id")]
    public int? SubjectId { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("duration_minutes")]
    public int? DurationMinutes { get; set; }

    [Column("total_questions")]
    public int? TotalQuestions { get; set; }

    [Column("year")]
    public short? Year { get; set; }

    [Column("exam_type")]
    [StringLength(50)]
    public string? ExamType { get; set; }

    [Column("is_timed")]
    public bool? IsTimed { get; set; }

    [Column("allow_pause")]
    public bool? AllowPause { get; set; }

    [Column("show_timer")]
    public bool? ShowTimer { get; set; }

    [Column("is_public")]
    public bool? IsPublic { get; set; }

    [Column("created_by")]
    public long? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("Exams")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("Exam")]
    public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();

    [ForeignKey("SubjectId")]
    [InverseProperty("Exams")]
    public virtual Subject? Subject { get; set; }

    [InverseProperty("Exam")]
    public virtual ICollection<UserAttempt> UserAttempts { get; set; } = new List<UserAttempt>();
}

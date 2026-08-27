using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[PrimaryKey("UserId", "LessonId")]
[Table("user_lesson_progress")]
public partial class UserLessonProgress
{
    [Key]
    [Column("user_id")]
    public long UserId { get; set; }

    [Key]
    [Column("lesson_id")]
    public int LessonId { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string? Status { get; set; }

    [Column("progress_percent")]
    public short? ProgressPercent { get; set; }

    [Column("last_position")]
    public int? LastPosition { get; set; }

    [Column("started_at")]
    public DateTime? StartedAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Column("last_accessed_at")]
    public DateTime? LastAccessedAt { get; set; }

    [ForeignKey("LessonId")]
    [InverseProperty("UserLessonProgresses")]
    public virtual Lesson Lesson { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserLessonProgresses")]
    public virtual User User { get; set; } = null!;
}

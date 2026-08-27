using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[Table("user_daily_progress")]
[Index("ProgressDate", Name = "idx_daily_progress_date")]
[Index("UserId", Name = "idx_daily_progress_user")]
[Index("UserId", "ProgressDate", Name = "user_daily_progress_user_id_progress_date_key", IsUnique = true)]
public partial class UserDailyProgress
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("progress_date")]
    public DateOnly ProgressDate { get; set; }

    [Column("questions_answered")]
    public int? QuestionsAnswered { get; set; }

    [Column("questions_correct")]
    public int? QuestionsCorrect { get; set; }

    [Column("study_minutes")]
    public int? StudyMinutes { get; set; }

    [Column("exams_completed")]
    public int? ExamsCompleted { get; set; }

    [Column("xp_earned")]
    public int? XpEarned { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserDailyProgresses")]
    public virtual User User { get; set; } = null!;
}

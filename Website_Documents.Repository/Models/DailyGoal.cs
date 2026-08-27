using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("daily_goals")]
public class DailyGoal
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("target_questions")]
    public int TargetQuestions { get; set; }

    [Column("target_minutes")]
    public int TargetMinutes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("DailyGoals")]
    public virtual User User { get; set; } = null!;
}

[Table("study_reminders")]
public class StudyReminder
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("title")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Column("reminder_time")]
    public TimeSpan ReminderTime { get; set; }

    [Column("days_of_week")]
    public string? DaysOfWeek { get; set; }

    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("StudyReminders")]
    public virtual User User { get; set; } = null!;
}

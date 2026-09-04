using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[Table("users")]
[Index("Email", Name = "users_email_key", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("email")]
    [StringLength(255)]
    public string Email { get; set; } = null!;

    // Alias property for compatibility
    [NotMapped]
    public string Username { get => Email; set => Email = value; }

    [Column("password_hash")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Column("full_name")]
    [StringLength(150)]
    public string? FullName { get; set; }

    [Column("role")]
    [StringLength(20)]
    public string? Role { get; set; }

    [Column("grade")]
    public short? Grade { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    // Gamification fields
    [Column("total_xp")]
    public int? TotalXp { get; set; }

    [Column("current_level")]
    public int? CurrentLevel { get; set; }

    [Column("current_streak")]
    public int? CurrentStreak { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    [InverseProperty("User")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("User")]
    public virtual ICollection<QuestionComment> QuestionComments { get; set; } = new List<QuestionComment>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    [InverseProperty("User")]
    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();

    [InverseProperty("User")]
    public virtual ICollection<UserAttempt> UserAttempts { get; set; } = new List<UserAttempt>();

    [InverseProperty("User")]
    public virtual ICollection<UserBookmark> UserBookmarks { get; set; } = new List<UserBookmark>();

    [InverseProperty("User")]
    public virtual ICollection<UserDailyProgress> UserDailyProgresses { get; set; } = new List<UserDailyProgress>();

    [InverseProperty("User")]
    public virtual ICollection<UserLessonProgress> UserLessonProgresses { get; set; } = new List<UserLessonProgress>();

    [InverseProperty("User")]
    public virtual ICollection<UserTopicProgress> UserTopicProgresses { get; set; } = new List<UserTopicProgress>();

    [InverseProperty("User")]
    public virtual ICollection<UserAnswerHistory> UserAnswerHistories { get; set; } = new List<UserAnswerHistory>();

    [InverseProperty("User")]
    public virtual ICollection<DailyGoal> DailyGoals { get; set; } = new List<DailyGoal>();

    [InverseProperty("User")]
    public virtual ICollection<StudyReminder> StudyReminders { get; set; } = new List<StudyReminder>();

    [InverseProperty("User")]
    public virtual ICollection<LearningPlan> LearningPlans { get; set; } = new List<LearningPlan>();

    [InverseProperty("User")]
    public virtual ICollection<ReviewSession> ReviewSessions { get; set; } = new List<ReviewSession>();

    [InverseProperty("User")]
    public virtual ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("live_session_members")]
public class LiveSessionMember
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("session_id")]
    public long SessionId { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("role")]
    [StringLength(20)]
    public string Role { get; set; } = "participant";

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = "joined";

    [Column("joined_at")]
    public DateTime? JoinedAt { get; set; }

    [Column("left_at")]
    public DateTime? LeftAt { get; set; }

    [Column("questions_answered")]
    public int QuestionsAnswered { get; set; } = 0;

    [Column("correct_answers")]
    public int CorrectAnswers { get; set; } = 0;

    [Column("total_score")]
    public int TotalScore { get; set; } = 0;

    [Column("current_streak")]
    public int CurrentStreak { get; set; } = 0;

    [Column("is_ready")]
    public bool IsReady { get; set; } = false;

    [Column("last_activity_at")]
    public DateTime? LastActivityAt { get; set; }

    [ForeignKey("SessionId")]
    public virtual LiveStudySession? Session { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}

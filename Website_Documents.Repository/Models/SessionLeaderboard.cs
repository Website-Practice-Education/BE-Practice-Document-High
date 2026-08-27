using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("session_leaderboard")]
public class SessionLeaderboard
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("session_id")]
    public long SessionId { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("rank_position")]
    public int RankPosition { get; set; } = 0;

    [Column("total_score")]
    public int TotalScore { get; set; } = 0;

    [Column("questions_correct")]
    public int QuestionsCorrect { get; set; } = 0;

    [Column("total_questions")]
    public int TotalQuestions { get; set; } = 0;

    [Column("average_time_seconds")]
    public int AverageTimeSeconds { get; set; } = 0;

    [Column("fastest_answer_seconds")]
    public int? FastestAnswerSeconds { get; set; }

    [Column("longest_streak")]
    public int LongestStreak { get; set; } = 0;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("SessionId")]
    public virtual LiveStudySession? Session { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}

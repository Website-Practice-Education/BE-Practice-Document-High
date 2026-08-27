using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("session_participant_answers")]
public class SessionParticipantAnswer
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("session_id")]
    public long SessionId { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("question_id")]
    public long QuestionId { get; set; }

    [Column("selected_option_id")]
    public long? SelectedOptionId { get; set; }

    [Column("selected_letter")]
    public char? SelectedLetter { get; set; }

    [Column("answer_text")]
    public string? AnswerText { get; set; }

    [Column("is_correct")]
    public bool? IsCorrect { get; set; }

    [Column("time_spent_seconds")]
    public int TimeSpentSeconds { get; set; } = 0;

    [Column("points_earned")]
    public int PointsEarned { get; set; } = 0;

    [Column("answered_at")]
    public DateTime? AnsweredAt { get; set; }

    [ForeignKey("SessionId")]
    public virtual LiveStudySession? Session { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [ForeignKey("QuestionId")]
    public virtual Question? Question { get; set; }

    [ForeignKey("SelectedOptionId")]
    public virtual QuestionOption? SelectedOption { get; set; }
}

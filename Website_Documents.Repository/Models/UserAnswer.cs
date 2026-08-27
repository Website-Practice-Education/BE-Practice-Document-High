using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[Table("user_answers")]
[Index("AttemptId", Name = "idx_user_answers_attempt")]
[Index("QuestionId", Name = "idx_user_answers_question")]
[Index("AttemptId", "QuestionId", Name = "user_answers_attempt_id_question_id_key", IsUnique = true)]
public partial class UserAnswer
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("attempt_id")]
    public long AttemptId { get; set; }

    [Column("question_id")]
    public long QuestionId { get; set; }

    [Column("selected_option_id")]
    public long? SelectedOptionId { get; set; }

    [Column("selected_option_letter")]
    [MaxLength(1)]
    public char? SelectedOptionLetter { get; set; }

    [Column("answer_text")]
    public string? AnswerText { get; set; }

    [Column("is_correct")]
    public bool? IsCorrect { get; set; }

    [Column("points_earned")]
    [Precision(5, 2)]
    public decimal? PointsEarned { get; set; }

    [Column("is_flagged")]
    public bool? IsFlagged { get; set; }

    [Column("time_spent_on_question")]
    public int? TimeSpentOnQuestion { get; set; }

    [Column("answered_at")]
    public DateTime? AnsweredAt { get; set; }

    [ForeignKey("AttemptId")]
    [InverseProperty("UserAnswers")]
    public virtual UserAttempt Attempt { get; set; } = null!;

    [ForeignKey("QuestionId")]
    [InverseProperty("UserAnswers")]
    public virtual Question Question { get; set; } = null!;

    [ForeignKey("SelectedOptionId")]
    [InverseProperty("UserAnswers")]
    public virtual QuestionOption? SelectedOption { get; set; }
}

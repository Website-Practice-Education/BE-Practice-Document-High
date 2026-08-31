using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[Table("user_answer_history")]
[Index("AttemptId", Name = "idx_answer_history_attempt")]
[Index("UserId", Name = "idx_answer_history_user")]
public partial class UserAnswerHistory
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long? UserId { get; set; }

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

    [Column("answered_at")]
    public DateTime? AnsweredAt { get; set; }

    [Column("changed_at")]
    public DateTime? ChangedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserAnswerHistories")]
    public virtual User? User { get; set; }

    [ForeignKey("AttemptId")]
    [InverseProperty("UserAnswerHistories")]
    public virtual UserAttempt Attempt { get; set; } = null!;

    [ForeignKey("QuestionId")]
    [InverseProperty("UserAnswerHistories")]
    public virtual Question Question { get; set; } = null!;

    [ForeignKey("SelectedOptionId")]
    [InverseProperty("UserAnswerHistories")]
    public virtual QuestionOption? SelectedOption { get; set; }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[Table("question_options")]
[Index("QuestionId", "OrderIndex", Name = "question_options_question_id_order_index_key", IsUnique = true)]
public partial class QuestionOption
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("question_id")]
    public long QuestionId { get; set; }

    [Column("content")]
    public string Content { get; set; } = null!;

    [Column("is_correct")]
    public bool? IsCorrect { get; set; }

    [Column("order_index")]
    public short? OrderIndex { get; set; }

    [ForeignKey("QuestionId")]
    [InverseProperty("QuestionOptions")]
    public virtual Question Question { get; set; } = null!;

    [InverseProperty("SelectedOption")]
    public virtual ICollection<UserAnswerHistory> UserAnswerHistories { get; set; } = new List<UserAnswerHistory>();

    [InverseProperty("SelectedOption")]
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}

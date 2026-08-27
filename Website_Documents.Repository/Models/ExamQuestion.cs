using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[PrimaryKey("ExamId", "QuestionId")]
[Table("exam_questions")]
[Index("ExamId", Name = "idx_exam_questions_exam")]
[Index("QuestionId", Name = "idx_exam_questions_question")]
public partial class ExamQuestion
{
    [Key]
    [Column("exam_id")]
    public long ExamId { get; set; }

    [Key]
    [Column("question_id")]
    public long QuestionId { get; set; }

    [Column("order_index")]
    public int? OrderIndex { get; set; }

    [Column("points")]
    [Precision(5, 2)]
    public decimal? Points { get; set; }

    [ForeignKey("ExamId")]
    [InverseProperty("ExamQuestions")]
    public virtual Exam Exam { get; set; } = null!;

    [ForeignKey("QuestionId")]
    [InverseProperty("ExamQuestions")]
    public virtual Question Question { get; set; } = null!;
}

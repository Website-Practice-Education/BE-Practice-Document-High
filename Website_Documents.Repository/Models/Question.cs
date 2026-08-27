using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[Table("questions")]
[Index("Difficulty", Name = "idx_questions_difficulty")]
[Index("LessonId", Name = "idx_questions_lesson")]
[Index("SubjectId", Name = "idx_questions_subject")]
[Index("TopicId", Name = "idx_questions_topic")]
[Index("Year", Name = "idx_questions_year")]
public partial class Question
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("subject_id")]
    public int SubjectId { get; set; }

    [Column("topic_id")]
    public int? TopicId { get; set; }

    [Column("lesson_id")]
    public int? LessonId { get; set; }

    [Column("question_type")]
    [StringLength(20)]
    public string QuestionType { get; set; } = null!;

    [Column("content")]
    public string Content { get; set; } = null!;

    [Column("explanation")]
    public string? Explanation { get; set; }

    [Column("difficulty")]
    public short? Difficulty { get; set; }

    [Column("year")]
    public short? Year { get; set; }

    [Column("source")]
    [StringLength(100)]
    public string? Source { get; set; }

    [Column("file_url")]
    [StringLength(500)]
    public string? FileUrl { get; set; }

    [Column("file_type")]
    [StringLength(50)]
    public string? FileType { get; set; }

    [Column("uploaded_by")]
    public long? UploadedBy { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("created_by")]
    public long? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("Questions")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("Question")]
    public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();

    [ForeignKey("LessonId")]
    [InverseProperty("Questions")]
    public virtual Lesson? Lesson { get; set; }

    [InverseProperty("Question")]
    public virtual ICollection<QuestionComment> QuestionComments { get; set; } = new List<QuestionComment>();

    [InverseProperty("Question")]
    public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();

    [ForeignKey("SubjectId")]
    [InverseProperty("Questions")]
    public virtual Subject Subject { get; set; } = null!;

    [ForeignKey("TopicId")]
    [InverseProperty("Questions")]
    public virtual Topic? Topic { get; set; }

    [InverseProperty("Question")]
    public virtual ICollection<UserAnswerHistory> UserAnswerHistories { get; set; } = new List<UserAnswerHistory>();

    [InverseProperty("Question")]
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    [InverseProperty("Question")]
    public virtual ICollection<UserBookmark> UserBookmarks { get; set; } = new List<UserBookmark>();
}

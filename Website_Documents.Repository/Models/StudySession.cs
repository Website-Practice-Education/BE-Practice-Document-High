using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("study_sessions")]
public class StudySession
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("subject_id")]
    public int SubjectId { get; set; }

    [Column("started_at")]
    public DateTime StartedAt { get; set; }

    [Column("ended_at")]
    public DateTime? EndedAt { get; set; }

    [Column("questions_answered")]
    public int QuestionsAnswered { get; set; }

    [Column("correct_answers")]
    public int CorrectAnswers { get; set; }

    [Column("time_spent_minutes")]
    public int TimeSpentMinutes { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = "active";

    [ForeignKey("UserId")]
    [InverseProperty("StudySessions")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("SubjectId")]
    [InverseProperty("StudySessions")]
    public virtual Subject Subject { get; set; } = null!;
}

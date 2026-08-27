using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("session_shared_questions")]
public class SessionSharedQuestion
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("session_id")]
    public long SessionId { get; set; }

    [Column("question_id")]
    public long QuestionId { get; set; }

    [Column("shared_by")]
    public long? SharedBy { get; set; }

    [Column("shared_at")]
    public DateTime? SharedAt { get; set; }

    [Column("order_index")]
    public int OrderIndex { get; set; } = 0;

    [Column("is_current")]
    public bool IsCurrent { get; set; } = false;

    [ForeignKey("SessionId")]
    public virtual LiveStudySession? Session { get; set; }

    [ForeignKey("QuestionId")]
    public virtual Question? Question { get; set; }

    [ForeignKey("SharedBy")]
    public virtual User? SharedByUser { get; set; }
}

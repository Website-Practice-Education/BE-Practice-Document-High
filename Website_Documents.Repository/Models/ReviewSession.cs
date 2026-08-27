using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("review_sessions")]
public class ReviewSession
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("started_at")]
    public DateTime StartedAt { get; set; }

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }

    [Column("cards_reviewed")]
    public int CardsReviewed { get; set; }

    [Column("correct_count")]
    public int CorrectCount { get; set; }

    [Column("total_time_seconds")]
    public int TotalTimeSeconds { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = "active";

    [ForeignKey("UserId")]
    [InverseProperty("ReviewSessions")]
    public virtual User User { get; set; } = null!;
}

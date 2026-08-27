using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("review_cards")]
public class ReviewCard
{
    [Key]
    [Column("user_id")]
    public long UserId { get; set; }

    [Key]
    [Column("question_id")]
    public long QuestionId { get; set; }

    [Column("next_review_date")]
    public DateTime NextReviewDate { get; set; }

    [Column("repetition_count")]
    public int RepetitionCount { get; set; }

    [Column("ease_factor")]
    public decimal EaseFactor { get; set; }

    [Column("interval_days")]
    public int IntervalDays { get; set; }

    [Column("last_review_date")]
    public DateTime LastReviewDate { get; set; }

    [Column("is_mastered")]
    public bool IsMastered { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("ReviewCards")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("QuestionId")]
    [InverseProperty("ReviewCards")]
    public virtual Question Question { get; set; } = null!;
}

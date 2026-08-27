using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[PrimaryKey("UserId", "TopicId")]
[Table("user_topic_progress")]
public partial class UserTopicProgress
{
    [Key]
    [Column("user_id")]
    public long UserId { get; set; }

    [Key]
    [Column("topic_id")]
    public int TopicId { get; set; }

    [Column("total_questions")]
    public int? TotalQuestions { get; set; }

    [Column("correct_count")]
    public int? CorrectCount { get; set; }

    [Column("last_practiced_at")]
    public DateTime? LastPracticedAt { get; set; }

    [ForeignKey("TopicId")]
    [InverseProperty("UserTopicProgresses")]
    public virtual Topic Topic { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserTopicProgresses")]
    public virtual User User { get; set; } = null!;
}

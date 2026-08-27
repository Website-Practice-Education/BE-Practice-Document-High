using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[Table("lessons")]
[Index("TopicId", Name = "idx_lessons_topic")]
[Index("Slug", Name = "lessons_slug_key", IsUnique = true)]
public partial class Lesson
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("topic_id")]
    public int TopicId { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("slug")]
    [StringLength(255)]
    public string? Slug { get; set; }

    [Column("content")]
    public string? Content { get; set; }

    [Column("video_url")]
    public string? VideoUrl { get; set; }

    [Column("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    [Column("order_index")]
    public int? OrderIndex { get; set; }

    [Column("estimated_minutes")]
    public int? EstimatedMinutes { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Lesson")]
    public virtual ICollection<LessonResource> LessonResources { get; set; } = new List<LessonResource>();

    [InverseProperty("Lesson")]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    [ForeignKey("TopicId")]
    [InverseProperty("Lessons")]
    public virtual Topic Topic { get; set; } = null!;

    [InverseProperty("Lesson")]
    public virtual ICollection<UserLessonProgress> UserLessonProgresses { get; set; } = new List<UserLessonProgress>();
}

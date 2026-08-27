using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[Table("lesson_resources")]
[Index("LessonId", Name = "idx_lesson_resources_lesson")]
public partial class LessonResource
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("lesson_id")]
    public int LessonId { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("resource_type")]
    [StringLength(30)]
    public string ResourceType { get; set; } = null!;

    [Column("resource_url")]
    public string ResourceUrl { get; set; } = null!;

    [Column("order_index")]
    public int? OrderIndex { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("LessonId")]
    [InverseProperty("LessonResources")]
    public virtual Lesson Lesson { get; set; } = null!;
}

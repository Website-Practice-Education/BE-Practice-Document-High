using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[Table("topics")]
[Index("SubjectId", Name = "idx_topics_subject")]
public partial class Topic
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("subject_id")]
    public int SubjectId { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("order_index")]
    public int? OrderIndex { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Parent")]
    public virtual ICollection<Topic> InverseParent { get; set; } = new List<Topic>();

    [InverseProperty("Topic")]
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    [ForeignKey("ParentId")]
    [InverseProperty("InverseParent")]
    public virtual Topic? Parent { get; set; }

    [InverseProperty("Topic")]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    [ForeignKey("SubjectId")]
    [InverseProperty("Topics")]
    public virtual Subject Subject { get; set; } = null!;

    [InverseProperty("Topic")]
    public virtual ICollection<UserAttempt> UserAttempts { get; set; } = new List<UserAttempt>();

    [InverseProperty("Topic")]
    public virtual ICollection<UserTopicProgress> UserTopicProgresses { get; set; } = new List<UserTopicProgress>();
}

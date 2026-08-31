using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[Table("subjects")]
[Index("Code", Name = "subjects_code_key", IsUnique = true)]
public partial class Subject
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("code")]
    [StringLength(20)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Subject")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    [InverseProperty("Subject")]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    [InverseProperty("Subject")]
    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();

    [InverseProperty("Subject")]
    public virtual ICollection<UserAttempt> UserAttempts { get; set; } = new List<UserAttempt>();

    [InverseProperty("Subject")]
    public virtual ICollection<LearningPlanItem> LearningPlanItems { get; set; } = new List<LearningPlanItem>();

    [InverseProperty("Subject")]
    public virtual ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();

    [InverseProperty("Subject")]
    public virtual ICollection<SharedDocument> SharedDocuments { get; set; } = new List<SharedDocument>();
}

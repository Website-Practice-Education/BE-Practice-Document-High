using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[Table("question_comments")]
[Index("QuestionId", Name = "idx_question_comments_question")]
public partial class QuestionComment
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("question_id")]
    public long QuestionId { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("parent_id")]
    public long? ParentId { get; set; }

    [Column("content")]
    public string Content { get; set; } = null!;

    [Column("is_deleted")]
    public bool? IsDeleted { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Parent")]
    public virtual ICollection<QuestionComment> InverseParent { get; set; } = new List<QuestionComment>();

    [ForeignKey("ParentId")]
    [InverseProperty("InverseParent")]
    public virtual QuestionComment? Parent { get; set; }

    [ForeignKey("QuestionId")]
    [InverseProperty("QuestionComments")]
    public virtual Question Question { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("QuestionComments")]
    public virtual User User { get; set; } = null!;
}

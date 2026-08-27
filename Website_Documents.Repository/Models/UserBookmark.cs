using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[PrimaryKey("UserId", "QuestionId")]
[Table("user_bookmarks")]
[Index("UserId", Name = "idx_user_bookmarks_user")]
public partial class UserBookmark
{
    [Key]
    [Column("user_id")]
    public long UserId { get; set; }

    [Key]
    [Column("question_id")]
    public long QuestionId { get; set; }

    [Column("note")]
    public string? Note { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("QuestionId")]
    [InverseProperty("UserBookmarks")]
    public virtual Question Question { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserBookmarks")]
    public virtual User User { get; set; } = null!;
}

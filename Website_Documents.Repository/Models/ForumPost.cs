using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

public class ForumPost
{
    [Key]
    public int Id { get; set; }

    [Required]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("content")]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    [Column("image_url")]
    public string? DocumentUrl { get; set; }
    
    // SharedLink is not in the database schema - this will be ignored or you need to add the column
    [NotMapped]
    public string? SharedLink { get; set; }

    [Column("like_count")]
    public int LikeCount { get; set; } = 0;

    [Column("comment_count")]
    public int CommentCount { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public ICollection<ForumComment> Comments { get; set; } = new List<ForumComment>();
    public ICollection<ForumLike> Likes { get; set; } = new List<ForumLike>();
}

public class ForumComment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PostId { get; set; }

    [ForeignKey(nameof(PostId))]
    public ForumPost? Post { get; set; }

    [Required]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    [MaxLength(1000)]
    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;
}

public class ForumLike
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PostId { get; set; }

    [ForeignKey(nameof(PostId))]
    public ForumPost? Post { get; set; }

    [Required]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Unique constraint: one user can like a post only once
}

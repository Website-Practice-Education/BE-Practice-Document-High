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
    public string Content { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? DocumentUrl { get; set; }
    
    [MaxLength(500)]
    public string? SharedLink { get; set; }

    public int LikeCount { get; set; } = 0;

    public int CommentCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

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
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Unique constraint: one user can like a post only once
}

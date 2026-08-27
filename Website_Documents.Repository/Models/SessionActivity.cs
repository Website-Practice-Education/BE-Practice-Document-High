using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("session_activities")]
public class SessionActivity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("session_id")]
    public long SessionId { get; set; }

    [Column("user_id")]
    public long? UserId { get; set; }

    [Column("activity_type")]
    [StringLength(30)]
    public string ActivityType { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("metadata", TypeName = "jsonb")]
    public string? Metadata { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("SessionId")]
    public virtual LiveStudySession? Session { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}

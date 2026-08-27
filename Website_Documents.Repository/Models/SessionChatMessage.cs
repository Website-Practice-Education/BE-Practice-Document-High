using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("session_chat_messages")]
public class SessionChatMessage
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("session_id")]
    public long SessionId { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("message_type")]
    [StringLength(20)]
    public string MessageType { get; set; } = "text";

    [Column("reply_to_id")]
    public long? ReplyToId { get; set; }

    [Column("is_pinned")]
    public bool IsPinned { get; set; } = false;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("SessionId")]
    public virtual LiveStudySession? Session { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [ForeignKey("ReplyToId")]
    public virtual SessionChatMessage? ReplyTo { get; set; }
}

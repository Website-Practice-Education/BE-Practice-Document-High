using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("chat_messages")]
public class ChatMessage
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("space_id")]
    public long SpaceId { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("content")]
    public string Content { get; set; } = null!;

    [Column("message_type")]
    [StringLength(20)]
    public string MessageType { get; set; } = "text"; // text, system, image

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("SpaceId")]
    public virtual StudySpace? Space { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}

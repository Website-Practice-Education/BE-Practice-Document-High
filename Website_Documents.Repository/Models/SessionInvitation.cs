using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("session_invitations")]
public class SessionInvitation
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("session_id")]
    public long SessionId { get; set; }

    [Column("invited_by")]
    public long InvitedBy { get; set; }

    [Column("invited_user_id")]
    public long? InvitedUserId { get; set; }

    [Column("invite_code")]
    [StringLength(20)]
    public string? InviteCode { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = "pending";

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("responded_at")]
    public DateTime? RespondedAt { get; set; }

    [ForeignKey("SessionId")]
    public virtual LiveStudySession? Session { get; set; }

    [ForeignKey("InvitedBy")]
    public virtual User? Inviter { get; set; }

    [ForeignKey("InvitedUserId")]
    public virtual User? InvitedUser { get; set; }
}

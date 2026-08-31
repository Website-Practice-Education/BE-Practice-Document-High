using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("call_sessions")]
public class CallSession
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("space_id")]
    public long SpaceId { get; set; }

    [Column("initiator_id")]
    public long InitiatorId { get; set; }

    [Column("call_type")]
    [MaxLength(20)]
    public string CallType { get; set; } = "audio"; // "audio" or "video"

    [Column("room_id")]
    [MaxLength(100)]
    public string RoomId { get; set; } = string.Empty;

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "active"; // "active", "ended", "missed"

    [Column("started_at")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    [Column("ended_at")]
    public DateTime? EndedAt { get; set; }

    [Column("max_participants")]
    public int MaxParticipants { get; set; } = 10;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SpaceId")]
    public virtual StudySpace? Space { get; set; }

    [ForeignKey("InitiatorId")]
    public virtual User? Initiator { get; set; }

    public virtual ICollection<CallParticipant> Participants { get; set; } = new List<CallParticipant>();
}

[Table("call_participants")]
public class CallParticipant
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("call_session_id")]
    public long CallSessionId { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("join_time")]
    public DateTime JoinTime { get; set; } = DateTime.UtcNow;

    [Column("leave_time")]
    public DateTime? LeaveTime { get; set; }

    [Column("is_muted")]
    public bool IsMuted { get; set; } = false;

    [Column("is_video_off")]
    public bool IsVideoOff { get; set; } = false;

    [Column("is_screen_sharing")]
    public bool IsScreenSharing { get; set; } = false;

    [Column("connection_status")]
    [MaxLength(20)]
    public string ConnectionStatus { get; set; } = "connected"; // "connected", "disconnected", "reconnecting"

    [Column("peer_id")]
    [MaxLength(100)]
    public string? PeerId { get; set; }

    // Navigation properties
    [ForeignKey("CallSessionId")]
    public virtual CallSession? CallSession { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[Table("notifications")]
[Index("IsRead", Name = "idx_notifications_read")]
[Index("UserId", Name = "idx_notifications_user")]
public partial class Notification
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("message")]
    public string Message { get; set; } = null!;

    // Alias property for compatibility (Content = Message)
    [NotMapped]
    public string Content
    {
        get => Message;
        set => Message = value;
    }

    [Column("notification_type")]
    [StringLength(30)]
    public string? NotificationType { get; set; }

    [Column("is_read")]
    public bool? IsRead { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("read_at")]
    public DateTime? ReadAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Notifications")]
    public virtual User User { get; set; } = null!;
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("friendships")]
public class Friendship
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("friend_id")]
    public long FriendId { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = "pending"; // pending, accepted, blocked

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [ForeignKey("FriendId")]
    public virtual User? Friend { get; set; }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("xp_transactions")]
public class XpTransaction
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("amount")]
    public int Amount { get; set; }

    [Column("reason")]
    [StringLength(100)]
    public string Reason { get; set; } = string.Empty;

    [Column("source_type")]
    [StringLength(50)]
    public string? SourceType { get; set; }

    [Column("source_id")]
    public long? SourceId { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }
}

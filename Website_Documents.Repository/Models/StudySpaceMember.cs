using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("study_space_members")]
public class StudySpaceMember
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("space_id")]
    public long SpaceId { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("role")]
    [StringLength(20)]
    public string Role { get; set; } = "member"; // owner, admin, member

    [Column("joined_at")]
    public DateTime? JoinedAt { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; } = true;

    [ForeignKey("SpaceId")]
    public virtual StudySpace? Space { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}

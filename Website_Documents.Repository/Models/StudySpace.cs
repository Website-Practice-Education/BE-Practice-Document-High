using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("study_spaces")]
public class StudySpace
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("space_type")]
    [StringLength(50)]
    public string SpaceType { get; set; } = "public"; // public, private, self_study

    [Column("invite_code")]
    [StringLength(20)]
    public string? InviteCode { get; set; }

    [Column("max_members")]
    public int MaxMembers { get; set; } = 50;

    [Column("is_active")]
    public bool? IsActive { get; set; } = true;

    [Column("created_by")]
    public long CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("CreatedBy")]
    public virtual User? Creator { get; set; }

    public virtual ICollection<StudySpaceMember> Members { get; set; } = new List<StudySpaceMember>();
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

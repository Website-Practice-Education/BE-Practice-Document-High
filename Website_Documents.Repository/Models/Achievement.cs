using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[Table("achievements")]
[Index("Code", Name = "achievements_code_key", IsUnique = true)]
public partial class Achievement
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("code")]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("icon_url")]
    public string? IconUrl { get; set; }

    [Column("xp_reward")]
    public int? XpReward { get; set; }

    [Column("condition_type")]
    [StringLength(50)]
    public string? ConditionType { get; set; }

    [Column("condition_value")]
    public int? ConditionValue { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Achievement")]
    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Documents.Repository.Models;

[PrimaryKey("UserId", "AchievementId")]
[Table("user_achievements")]
public partial class UserAchievement
{
    [Key]
    [Column("user_id")]
    public long UserId { get; set; }

    [Key]
    [Column("achievement_id")]
    public int AchievementId { get; set; }

    [Column("achieved_at")]
    public DateTime? AchievedAt { get; set; }

    [ForeignKey("AchievementId")]
    [InverseProperty("UserAchievements")]
    public virtual Achievement Achievement { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserAchievements")]
    public virtual User User { get; set; } = null!;
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("learning_plans")]
public class LearningPlan
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("target_days")]
    public int TargetDays { get; set; }

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    [Column("daily_target_questions")]
    public int DailyTargetQuestions { get; set; }

    [Column("daily_target_minutes")]
    public int DailyTargetMinutes { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("LearningPlans")]
    public virtual User User { get; set; } = null!;

    [InverseProperty("Plan")]
    public virtual ICollection<LearningPlanItem> Items { get; set; } = new List<LearningPlanItem>();
}

[Table("learning_plan_items")]
public class LearningPlanItem
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("plan_id")]
    public long PlanId { get; set; }

    [Column("subject_id")]
    public int SubjectId { get; set; }

    [Column("topic_id")]
    public int? TopicId { get; set; }

    [Column("priority")]
    public int Priority { get; set; }

    [Column("target_questions")]
    public int TargetQuestions { get; set; }

    [Column("completed_questions")]
    public int CompletedQuestions { get; set; }

    [Column("is_completed")]
    public bool IsCompleted { get; set; }

    [ForeignKey("PlanId")]
    [InverseProperty("Items")]
    public virtual LearningPlan Plan { get; set; } = null!;

    [ForeignKey("SubjectId")]
    [InverseProperty("LearningPlanItems")]
    public virtual Subject Subject { get; set; } = null!;

    [ForeignKey("TopicId")]
    [InverseProperty("LearningPlanItems")]
    public virtual Topic? Topic { get; set; }
}

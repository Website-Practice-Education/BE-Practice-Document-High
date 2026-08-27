using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Documents.Repository.Models;

[Table("live_study_sessions")]
public class LiveStudySession
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("space_id")]
    public long? SpaceId { get; set; }

    [Column("title")]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("session_type")]
    [StringLength(30)]
    public string SessionType { get; set; } = "practice";

    [Column("subject_id")]
    public int? SubjectId { get; set; }

    [Column("topic_id")]
    public int? TopicId { get; set; }

    [Column("difficulty_level")]
    public short DifficultyLevel { get; set; } = 1;

    [Column("question_count")]
    public int QuestionCount { get; set; } = 10;

    [Column("time_limit_minutes")]
    public int TimeLimitMinutes { get; set; } = 30;

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = "waiting";

    [Column("max_participants")]
    public int MaxParticipants { get; set; } = 20;

    [Column("current_participants")]
    public int CurrentParticipants { get; set; } = 0;

    [Column("invite_code")]
    [StringLength(20)]
    public string? InviteCode { get; set; }

    [Column("host_id")]
    public long? HostId { get; set; }

    [Column("started_at")]
    public DateTime? StartedAt { get; set; }

    [Column("ended_at")]
    public DateTime? EndedAt { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("SpaceId")]
    public virtual StudySpace? Space { get; set; }

    [ForeignKey("SubjectId")]
    public virtual Subject? Subject { get; set; }

    [ForeignKey("TopicId")]
    public virtual Topic? Topic { get; set; }

    [ForeignKey("HostId")]
    public virtual User? Host { get; set; }

    public virtual ICollection<LiveSessionMember> Members { get; set; } = new List<LiveSessionMember>();
    public virtual ICollection<SessionActivity> Activities { get; set; } = new List<SessionActivity>();
    public virtual ICollection<SessionWhiteboard> WhiteboardItems { get; set; } = new List<SessionWhiteboard>();
    public virtual ICollection<SessionChatMessage> ChatMessages { get; set; } = new List<SessionChatMessage>();
    public virtual ICollection<SessionSharedQuestion> SharedQuestions { get; set; } = new List<SessionSharedQuestion>();
    public virtual ICollection<SessionParticipantAnswer> ParticipantAnswers { get; set; } = new List<SessionParticipantAnswer>();
    public virtual ICollection<SessionLeaderboard> Leaderboard { get; set; } = new List<SessionLeaderboard>();
}

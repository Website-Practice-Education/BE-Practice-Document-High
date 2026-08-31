using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository.DBContext;

public partial class BookstoreDbContext : DbContext
{
    public BookstoreDbContext(DbContextOptions<BookstoreDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Achievement> Achievements { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<ExamQuestion> ExamQuestions { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonResource> LessonResources { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionComment> QuestionComments { get; set; }

    public virtual DbSet<QuestionOption> QuestionOptions { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAchievement> UserAchievements { get; set; }

    public virtual DbSet<UserAnswer> UserAnswers { get; set; }

    public virtual DbSet<UserAnswerHistory> UserAnswerHistories { get; set; }

    public virtual DbSet<UserAttempt> UserAttempts { get; set; }

    public virtual DbSet<UserBookmark> UserBookmarks { get; set; }

    public virtual DbSet<UserDailyProgress> UserDailyProgresses { get; set; }

    public virtual DbSet<UserLessonProgress> UserLessonProgresses { get; set; }

    public virtual DbSet<UserTopicProgress> UserTopicProgresses { get; set; }

    public virtual DbSet<StudySpace> StudySpaces { get; set; }

    public virtual DbSet<StudySpaceMember> StudySpaceMembers { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<Friendship> Friendships { get; set; }

    public virtual DbSet<LiveStudySession> LiveStudySessions { get; set; }

    public virtual DbSet<LiveSessionMember> LiveSessionMembers { get; set; }

    public virtual DbSet<SessionActivity> SessionActivities { get; set; }

    public virtual DbSet<SessionWhiteboard> SessionWhiteboards { get; set; }

    public virtual DbSet<SessionChatMessage> SessionChatMessages { get; set; }

    public virtual DbSet<SessionSharedQuestion> SessionSharedQuestions { get; set; }

    public virtual DbSet<SessionParticipantAnswer> SessionParticipantAnswers { get; set; }

    public virtual DbSet<SessionLeaderboard> SessionLeaderboards { get; set; }

    public virtual DbSet<SessionInvitation> SessionInvitations { get; set; }

    public virtual DbSet<LearningPlan> LearningPlans { get; set; }

    public virtual DbSet<DailyGoal> DailyGoals { get; set; }

    public virtual DbSet<ReviewCard> ReviewCards { get; set; }

    public virtual DbSet<StudySession> StudySessions { get; set; }

    public virtual DbSet<ReviewSession> ReviewSessions { get; set; }

    public virtual DbSet<StudyReminder> StudyReminders { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<RoomMusicTrack> RoomMusicTracks { get; set; }

    public virtual DbSet<RoomSharedFile> RoomSharedFiles { get; set; }

    public virtual DbSet<RoomSetting> RoomSettings { get; set; }

    public virtual DbSet<SharedDocument> SharedDocuments { get; set; }

    public virtual DbSet<ForumPost> ForumPosts { get; set; }

    public virtual DbSet<ForumComment> ForumComments { get; set; }

    public virtual DbSet<ForumLike> ForumLikes { get; set; }

    public virtual DbSet<CallSession> CallSessions { get; set; }

    public virtual DbSet<CallParticipant> CallParticipants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("achievements_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.XpReward).HasDefaultValue(0);
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("exams_pkey");

            entity.Property(e => e.AllowPause).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.ExamType).HasDefaultValueSql("'practice'::character varying");
            entity.Property(e => e.IsPublic).HasDefaultValue(true);
            entity.Property(e => e.IsTimed).HasDefaultValue(true);
            entity.Property(e => e.ShowTimer).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Exams)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("exams_created_by_fkey");

            entity.HasOne(d => d.Subject).WithMany(p => p.Exams)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("exams_subject_id_fkey");
        });

        modelBuilder.Entity<ExamQuestion>(entity =>
        {
            entity.HasKey(e => new { e.ExamId, e.QuestionId }).HasName("exam_questions_pkey");

            entity.Property(e => e.OrderIndex).HasDefaultValue(0);
            entity.Property(e => e.Points).HasDefaultValueSql("1.0");

            entity.HasOne(d => d.Exam).WithMany(p => p.ExamQuestions).HasConstraintName("exam_questions_exam_id_fkey");

            entity.HasOne(d => d.Question).WithMany(p => p.ExamQuestions).HasConstraintName("exam_questions_question_id_fkey");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lessons_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OrderIndex).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Topic).WithMany(p => p.Lessons).HasConstraintName("lessons_topic_id_fkey");
        });

        modelBuilder.Entity<LessonResource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lesson_resources_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.OrderIndex).HasDefaultValue(0);

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonResources).HasConstraintName("lesson_resources_lesson_id_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notifications_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsRead).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications).HasConstraintName("notifications_user_id_fkey");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("questions_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Difficulty).HasDefaultValue((short)1);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Questions)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("questions_created_by_fkey");

            entity.HasOne(d => d.Lesson).WithMany(p => p.Questions)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("questions_lesson_id_fkey");

            entity.HasOne(d => d.Subject).WithMany(p => p.Questions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("questions_subject_id_fkey");

            entity.HasOne(d => d.Topic).WithMany(p => p.Questions)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("questions_topic_id_fkey");
        });

        modelBuilder.Entity<QuestionComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("question_comments_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("question_comments_parent_id_fkey");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionComments).HasConstraintName("question_comments_question_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.QuestionComments).HasConstraintName("question_comments_user_id_fkey");
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("question_options_pkey");

            entity.Property(e => e.IsCorrect).HasDefaultValue(false);
            entity.Property(e => e.OrderIndex).HasDefaultValue((short)0);

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionOptions).HasConstraintName("question_options_question_id_fkey");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subjects_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("topics_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OrderIndex).HasDefaultValue(0);

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("topics_parent_id_fkey");

            entity.HasOne(d => d.Subject).WithMany(p => p.Topics).HasConstraintName("topics_subject_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Role).HasDefaultValueSql("'student'::character varying");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.AchievementId }).HasName("user_achievements_pkey");

            entity.Property(e => e.AchievedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Achievement).WithMany(p => p.UserAchievements).HasConstraintName("user_achievements_achievement_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserAchievements).HasConstraintName("user_achievements_user_id_fkey");
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_answers_pkey");

            entity.Property(e => e.AnsweredAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsFlagged).HasDefaultValue(false);
            entity.Property(e => e.PointsEarned).HasDefaultValueSql("0");

            entity.HasOne(d => d.Attempt).WithMany(p => p.UserAnswers).HasConstraintName("user_answers_attempt_id_fkey");

            entity.HasOne(d => d.Question).WithMany(p => p.UserAnswers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_answers_question_id_fkey");

            entity.HasOne(d => d.SelectedOption).WithMany(p => p.UserAnswers)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_answers_selected_option_id_fkey");
        });

        modelBuilder.Entity<UserAnswerHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_answer_history_pkey");

            entity.Property(e => e.ChangedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Attempt).WithMany(p => p.UserAnswerHistories).HasConstraintName("user_answer_history_attempt_id_fkey");

            entity.HasOne(d => d.Question).WithMany(p => p.UserAnswerHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_answer_history_question_id_fkey");

            entity.HasOne(d => d.SelectedOption).WithMany(p => p.UserAnswerHistories)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_answer_history_selected_option_id_fkey");
        });

        modelBuilder.Entity<UserAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_attempts_pkey");

            entity.Property(e => e.IsTimeout).HasDefaultValue(false);
            entity.Property(e => e.StartedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Status).HasDefaultValueSql("'in_progress'::character varying");
            entity.Property(e => e.SubmittedBy).HasDefaultValueSql("'user'::character varying");

            entity.HasOne(d => d.Exam).WithMany(p => p.UserAttempts)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_attempts_exam_id_fkey");

            entity.HasOne(d => d.Subject).WithMany(p => p.UserAttempts)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_attempts_subject_id_fkey");

            entity.HasOne(d => d.Topic).WithMany(p => p.UserAttempts)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_attempts_topic_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserAttempts).HasConstraintName("user_attempts_user_id_fkey");
        });

        modelBuilder.Entity<UserBookmark>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.QuestionId }).HasName("user_bookmarks_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Question).WithMany(p => p.UserBookmarks).HasConstraintName("user_bookmarks_question_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserBookmarks).HasConstraintName("user_bookmarks_user_id_fkey");
        });

        modelBuilder.Entity<UserDailyProgress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_daily_progress_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.ExamsCompleted).HasDefaultValue(0);
            entity.Property(e => e.QuestionsAnswered).HasDefaultValue(0);
            entity.Property(e => e.QuestionsCorrect).HasDefaultValue(0);
            entity.Property(e => e.StudyMinutes).HasDefaultValue(0);
            entity.Property(e => e.XpEarned).HasDefaultValue(0);

            entity.HasOne(d => d.User).WithMany(p => p.UserDailyProgresses).HasConstraintName("user_daily_progress_user_id_fkey");
        });

        modelBuilder.Entity<UserLessonProgress>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LessonId }).HasName("user_lesson_progress_pkey");

            entity.Property(e => e.LastAccessedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.LastPosition).HasDefaultValue(0);
            entity.Property(e => e.ProgressPercent).HasDefaultValue((short)0);
            entity.Property(e => e.Status).HasDefaultValueSql("'not_started'::character varying");

            entity.HasOne(d => d.Lesson).WithMany(p => p.UserLessonProgresses).HasConstraintName("user_lesson_progress_lesson_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserLessonProgresses).HasConstraintName("user_lesson_progress_user_id_fkey");
        });

        modelBuilder.Entity<UserTopicProgress>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.TopicId }).HasName("user_topic_progress_pkey");

            entity.Property(e => e.CorrectCount).HasDefaultValue(0);
            entity.Property(e => e.TotalQuestions).HasDefaultValue(0);

            entity.HasOne(d => d.Topic).WithMany(p => p.UserTopicProgresses).HasConstraintName("user_topic_progress_topic_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserTopicProgresses).HasConstraintName("user_topic_progress_user_id_fkey");
        });

        modelBuilder.Entity<StudySpace>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("study_spaces_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SpaceType).HasDefaultValueSql("'public'::character varying");

            entity.HasOne(d => d.Creator).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("study_spaces_created_by_fkey");
        });

        modelBuilder.Entity<StudySpaceMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("study_space_members_pkey");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.JoinedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Role).HasDefaultValueSql("'member'::character varying");

            entity.HasOne(d => d.Space).WithMany(p => p.Members)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("study_space_members_space_id_fkey");

            entity.HasOne(d => d.User).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("study_space_members_user_id_fkey");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("chat_messages_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.MessageType).HasDefaultValueSql("'text'::character varying");

            entity.HasOne(d => d.Space).WithMany(p => p.Messages)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("chat_messages_space_id_fkey");

            entity.HasOne(d => d.User).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("chat_messages_user_id_fkey");
        });

        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("friendships_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Status).HasDefaultValueSql("'pending'::character varying");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.User).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("friendships_user_id_fkey");

            entity.HasOne(d => d.Friend).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("friendships_friend_id_fkey");
        });

        modelBuilder.Entity<LiveStudySession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("live_study_sessions_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.DifficultyLevel).HasDefaultValue((short)1);
            entity.Property(e => e.QuestionCount).HasDefaultValue(10);
            entity.Property(e => e.SessionType).HasDefaultValueSql("'practice'::character varying");
            entity.Property(e => e.Status).HasDefaultValueSql("'waiting'::character varying");
            entity.Property(e => e.MaxParticipants).HasDefaultValue(20);
            entity.Property(e => e.CurrentParticipants).HasDefaultValue(0);
            entity.Property(e => e.TimeLimitMinutes).HasDefaultValue(30);

            entity.HasOne(d => d.Space).WithMany(p => p.LiveSessions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("live_sessions_space_id_fkey");

            entity.HasOne(d => d.Host).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("live_sessions_host_id_fkey");

            entity.HasOne(d => d.Subject).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("live_sessions_subject_id_fkey");

            entity.HasOne(d => d.Topic).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("live_sessions_topic_id_fkey");
        });

        modelBuilder.Entity<LiveSessionMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("live_session_members_pkey");

            entity.Property(e => e.JoinedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.LastActivityAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Role).HasDefaultValueSql("'participant'::character varying");
            entity.Property(e => e.Status).HasDefaultValueSql("'joined'::character varying");
            entity.Property(e => e.IsReady).HasDefaultValue(false);
            entity.Property(e => e.QuestionsAnswered).HasDefaultValue(0);
            entity.Property(e => e.CorrectAnswers).HasDefaultValue(0);
            entity.Property(e => e.TotalScore).HasDefaultValue(0);
            entity.Property(e => e.CurrentStreak).HasDefaultValue(0);

            entity.HasOne(d => d.Session).WithMany(p => p.Members)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("session_members_session_id_fkey");

            entity.HasOne(d => d.User).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("session_members_user_id_fkey");
        });

        modelBuilder.Entity<SessionActivity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("session_activities_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Session).WithMany(p => p.Activities)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("session_activities_session_id_fkey");

            entity.HasOne(d => d.User).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("session_activities_user_id_fkey");
        });

        modelBuilder.Entity<SessionWhiteboard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("session_whiteboard_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.PositionX).HasDefaultValue(0);
            entity.Property(e => e.PositionY).HasDefaultValue(0);
            entity.Property(e => e.LayerIndex).HasDefaultValue(0);
            entity.Property(e => e.IsLocked).HasDefaultValue(false);

            entity.HasOne(d => d.Session).WithMany(p => p.WhiteboardItems)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("whiteboard_session_id_fkey");

            entity.HasOne(d => d.User).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("whiteboard_user_id_fkey");
        });

        modelBuilder.Entity<SessionChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("session_chat_messages_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.MessageType).HasDefaultValueSql("'text'::character varying");
            entity.Property(e => e.IsPinned).HasDefaultValue(false);

            entity.HasOne(d => d.Session).WithMany(p => p.ChatMessages)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("session_chat_session_id_fkey");

            entity.HasOne(d => d.User).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("session_chat_user_id_fkey");

            entity.HasOne(d => d.ReplyTo).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("session_chat_reply_to_fkey");
        });

        modelBuilder.Entity<SessionSharedQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("session_shared_questions_pkey");

            entity.Property(e => e.SharedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.OrderIndex).HasDefaultValue(0);
            entity.Property(e => e.IsCurrent).HasDefaultValue(false);

            entity.HasOne(d => d.Session).WithMany(p => p.SharedQuestions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("shared_questions_session_id_fkey");

            entity.HasOne(d => d.Question).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("shared_questions_question_id_fkey");

            entity.HasOne(d => d.SharedByUser).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("shared_questions_shared_by_fkey");
        });

        modelBuilder.Entity<SessionParticipantAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("session_participant_answers_pkey");

            entity.Property(e => e.AnsweredAt).HasDefaultValueSql("now()");
            entity.Property(e => e.TimeSpentSeconds).HasDefaultValue(0);
            entity.Property(e => e.PointsEarned).HasDefaultValue(0);

            entity.HasOne(d => d.Session).WithMany(p => p.ParticipantAnswers)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("participant_answers_session_id_fkey");

            entity.HasOne(d => d.User).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("participant_answers_user_id_fkey");

            entity.HasOne(d => d.Question).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("participant_answers_question_id_fkey");

            entity.HasOne(d => d.SelectedOption).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("participant_answers_option_id_fkey");
        });

        modelBuilder.Entity<SessionLeaderboard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("session_leaderboard_pkey");

            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.RankPosition).HasDefaultValue(0);
            entity.Property(e => e.TotalScore).HasDefaultValue(0);
            entity.Property(e => e.QuestionsCorrect).HasDefaultValue(0);
            entity.Property(e => e.TotalQuestions).HasDefaultValue(0);
            entity.Property(e => e.AverageTimeSeconds).HasDefaultValue(0);
            entity.Property(e => e.LongestStreak).HasDefaultValue(0);

            entity.HasOne(d => d.Session).WithMany(p => p.Leaderboard)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("leaderboard_session_id_fkey");

            entity.HasOne(d => d.User).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("leaderboard_user_id_fkey");
        });

        modelBuilder.Entity<SessionInvitation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("session_invitations_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Status).HasDefaultValueSql("'pending'::character varying");

            entity.HasOne(d => d.Session).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("session_invitations_session_id_fkey");

            entity.HasOne(d => d.Inviter).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("session_invitations_invited_by_fkey");

            entity.HasOne(d => d.InvitedUser).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("session_invitations_invited_user_id_fkey");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("password_reset_tokens_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsUsed).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("password_reset_tokens_user_id_fkey");
        });

        modelBuilder.Entity<ReviewCard>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.QuestionId }).HasName("review_cards_pkey");

            entity.HasOne(d => d.Question).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("review_cards_question_id_fkey");

            entity.HasOne(d => d.User).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("review_cards_user_id_fkey");
        });

        modelBuilder.Entity<StudySession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("study_sessions_pkey");

            entity.Property(e => e.StartedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Status).HasDefaultValueSql("'active'::character varying");

            entity.HasOne(d => d.Subject).WithMany(p => p.StudySessions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("study_sessions_subject_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.StudySessions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("study_sessions_user_id_fkey");
        });

        modelBuilder.Entity<ReviewSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("review_sessions_pkey");

            entity.Property(e => e.Status).HasDefaultValueSql("'active'::character varying");

            entity.HasOne(d => d.User).WithMany(p => p.ReviewSessions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("review_sessions_user_id_fkey");
        });

        // Additional model configurations for Room Features
        modelBuilder.Entity<RoomMusicTrack>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("room_music_tracks_pkey");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Space).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("room_music_tracks_space_id_fkey");

            entity.HasOne(d => d.Uploader).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("room_music_tracks_uploaded_by_fkey");
        });

        modelBuilder.Entity<RoomSharedFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("room_shared_files_pkey");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Space).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("room_shared_files_space_id_fkey");

            entity.HasOne(d => d.Uploader).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("room_shared_files_uploaded_by_fkey");
        });

        modelBuilder.Entity<RoomSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("room_settings_pkey");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Space).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("room_settings_space_id_fkey");

            entity.HasOne(d => d.Updater).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("room_settings_updated_by_fkey");
        });

        modelBuilder.Entity<SharedDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shared_documents_pkey");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.DocumentType).HasDefaultValueSql("'link'::character varying");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.ModerationStatus).HasDefaultValueSql("'pending'::character varying");
            entity.Property(e => e.ViewCount).HasDefaultValue(0);
            entity.Property(e => e.DownloadCount).HasDefaultValue(0);
            entity.Property(e => e.LikeCount).HasDefaultValue(0);

            entity.HasOne(d => d.Subject).WithMany(p => p.SharedDocuments)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("shared_documents_subject_id_fkey");

            entity.HasOne(d => d.Topic).WithMany(p => p.SharedDocuments)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("shared_documents_topic_id_fkey");

            entity.HasOne(d => d.SharedByUser).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("shared_documents_user_id_fkey");
        });

        // Forum Post configurations
        modelBuilder.Entity<ForumPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("forum_posts_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.LikeCount).HasDefaultValue(0);
            entity.Property(e => e.CommentCount).HasDefaultValue(0);

            entity.HasOne(d => d.User).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("forum_posts_user_id_fkey");
        });

        modelBuilder.Entity<ForumComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("forum_comments_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("forum_comments_post_id_fkey");

            entity.HasOne(d => d.User).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("forum_comments_user_id_fkey");
        });

        modelBuilder.Entity<ForumLike>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("forum_likes_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasIndex(e => new { e.PostId, e.UserId }).IsUnique();

            entity.HasOne(d => d.Post).WithMany(p => p.Likes)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("forum_likes_post_id_fkey");

            entity.HasOne(d => d.User).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("forum_likes_user_id_fkey");
        });

        modelBuilder.Entity<CallSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("call_sessions_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.StartedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.CallType).HasDefaultValueSql("'audio'::character varying");
            entity.Property(e => e.Status).HasDefaultValueSql("'active'::character varying");
            entity.Property(e => e.MaxParticipants).HasDefaultValue(10);

            entity.HasOne(d => d.Space).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("call_sessions_space_id_fkey");

            entity.HasOne(d => d.Initiator).WithMany()
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("call_sessions_initiator_id_fkey");
        });

        modelBuilder.Entity<CallParticipant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("call_participants_pkey");

            entity.Property(e => e.JoinTime).HasDefaultValueSql("now()");
            entity.Property(e => e.IsMuted).HasDefaultValue(false);
            entity.Property(e => e.IsVideoOff).HasDefaultValue(false);
            entity.Property(e => e.IsScreenSharing).HasDefaultValue(false);
            entity.Property(e => e.ConnectionStatus).HasDefaultValueSql("'connected'::character varying");

            entity.HasOne(d => d.CallSession).WithMany(p => p.Participants)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("call_participants_session_id_fkey");

            entity.HasOne(d => d.User).WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("call_participants_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

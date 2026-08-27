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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

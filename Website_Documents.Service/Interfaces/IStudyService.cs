using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface IStudyService
{
    // ===== Study Session =====
    Task<StudySession> StartStudySessionAsync(long userId, int subjectId);
    Task<StudySession?> GetStudySessionAsync(long sessionId);
    Task<StudySession?> GetActiveSessionAsync(long userId);
    Task<StudySession> EndStudySessionAsync(long sessionId);
    Task UpdateStudySessionProgressAsync(long sessionId, int questionsAnswered, int correctAnswers, int timeSpentMinutes);

    // ===== Practice Questions =====
    Task<List<Question>> GetPracticeQuestionsAsync(long userId, int subjectId, int topicId, int count, short? minDifficulty, short? maxDifficulty);
    Task<List<Question>> GetWeakQuestionsAsync(long userId, int count);
    Task<List<Question>> GetRecommendedQuestionsAsync(long userId, int count);
    
    // ===== Quiz Mode =====
    Task<QuizResult> StartQuizAsync(long userId, int subjectId, int questionCount, string difficulty);
    Task<bool> SubmitQuizAnswerAsync(long sessionId, long questionId, long? selectedOptionId, bool isCorrect, int timeSpentSeconds);
    Task<QuizResult> CompleteQuizAsync(long sessionId);

    // ===== Statistics =====
    Task<StudyStatistics> GetStudyStatisticsAsync(long userId);
    Task<List<TopicMastery>> GetTopicMasteryAsync(long userId);
    Task<SubjectProgress> GetSubjectProgressAsync(long userId, int subjectId);
}

public class StudySession
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public int SubjectId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int QuestionsAnswered { get; set; }
    public int CorrectAnswers { get; set; }
    public int TimeSpentMinutes { get; set; }
    public string Status { get; set; } = "active";
}

public class QuizResult
{
    public long SessionId { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int TimeSpentMinutes { get; set; }
    public decimal ScorePercentage { get; set; }
    public List<QuestionResult> QuestionResults { get; set; } = new();
    public DateTime CompletedAt { get; set; }
}

public class QuestionResult
{
    public long QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public long? SelectedOptionId { get; set; }
    public int TimeSpentSeconds { get; set; }
}

public class StudyStatistics
{
    public int TotalQuestionsAnswered { get; set; }
    public int TotalCorrectAnswers { get; set; }
    public decimal OverallAccuracy { get; set; }
    public int TotalStudyTimeMinutes { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int TotalStudyDays { get; set; }
    public decimal AverageSessionMinutes { get; set; }
    public int QuestionsToday { get; set; }
    public int CorrectToday { get; set; }
    public List<WeakTopic> WeakTopics { get; set; } = new();
    public List<StrongTopic> StrongTopics { get; set; } = new();
}

public class WeakTopic
{
    public int TopicId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public decimal Accuracy { get; set; }
}

public class StrongTopic
{
    public int TopicId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public decimal Accuracy { get; set; }
}

public class TopicMastery
{
    public int TopicId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public decimal MasteryPercentage { get; set; }
    public string MasteryLevel { get; set; } = "Beginner";
}

public class SubjectProgress
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int AttemptedQuestions { get; set; }
    public int MasteredQuestions { get; set; }
    public decimal CompletionPercentage { get; set; }
    public List<TopicProgress> TopicProgresses { get; set; } = new();
}

public class TopicProgress
{
    public int TopicId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int Attempted { get; set; }
    public int Mastered { get; set; }
    public decimal MasteryPercentage { get; set; }
}

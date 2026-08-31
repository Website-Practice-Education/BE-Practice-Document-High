using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Website_Documents.Service.Interfaces;

public interface IExamAttemptService
{
    Task<ExamAttemptResponse> StartExamAsync(long userId, long examId);
    Task<ExamAttemptResponse> SubmitAnswerAsync(long attemptId, long questionId, long? selectedOptionId);
    Task<ExamAttemptResponse> SubmitExamAsync(long attemptId);
    Task<ExamAttemptResponse?> GetAttemptByIdAsync(long attemptId);
    Task<List<ExamAttemptResponse>> GetUserAttemptsAsync(long userId);
    Task<ExamResultResponse> GetExamResultAsync(long attemptId);
}

public class ExamAttemptResponse
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? ExamId { get; set; }
    public string? ExamTitle { get; set; }
    public decimal? Score { get; set; }
    public int? TotalCorrect { get; set; }
    public int? TotalQuestions { get; set; }
    public string Status { get; set; } = "in_progress";
    public DateTime? StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int? TimeSpentSeconds { get; set; }
}

public class ExamResultResponse
{
    public long AttemptId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public decimal? Percentage => TotalQuestions > 0 && TotalQuestions.HasValue ? (decimal)(TotalCorrect ?? 0) / TotalQuestions.Value * 100 : 0;
    public int? TotalCorrect { get; set; }
    public int? TotalQuestions { get; set; }
    public string Grade => (Percentage ?? 0) >= 90 ? "A" : (Percentage ?? 0) >= 80 ? "B" : (Percentage ?? 0) >= 70 ? "C" : (Percentage ?? 0) >= 60 ? "D" : "F";
    public DateTime? StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int? TimeSpentSeconds { get; set; }
    public List<QuestionResultDto> QuestionResults { get; set; } = new();
}

public class QuestionResultDto
{
    public long QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? SelectedAnswer { get; set; }
    public string? CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class ExamAttemptService : IExamAttemptService
{
    private readonly BookstoreDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public ExamAttemptService(BookstoreDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<ExamAttemptResponse> StartExamAsync(long userId, long examId)
    {
        var exam = await _context.Exams.FindAsync(examId);
        if (exam == null)
            throw new KeyNotFoundException("Exam not found");

        var attempt = new UserAttempt
        {
            UserId = userId,
            ExamId = examId,
            SubjectId = exam.SubjectId,
            Status = "in_progress",
            StartedAt = DateTime.UtcNow
        };

        _context.UserAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        return MapToResponse(attempt, exam.Title);
    }

    public async Task<ExamAttemptResponse> SubmitAnswerAsync(long attemptId, long questionId, long? selectedOptionId)
    {
        var attempt = await _context.UserAttempts
            .Include(a => a.Exam)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        if (attempt == null)
            throw new KeyNotFoundException("Attempt not found");

        if (attempt.Status != "in_progress")
            throw new InvalidOperationException("Exam already submitted");

        var existingAnswer = await _context.UserAnswers
            .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.QuestionId == questionId);

        if (existingAnswer != null)
        {
            existingAnswer.SelectedOptionId = selectedOptionId;
            existingAnswer.AnsweredAt = DateTime.UtcNow;
        }
        else
        {
            bool isCorrect = false;
            if (selectedOptionId.HasValue)
            {
                var option = await _context.QuestionOptions.FindAsync(selectedOptionId.Value);
                isCorrect = option?.IsCorrect ?? false;
            }

            var answer = new UserAnswer
            {
                AttemptId = attemptId,
                QuestionId = questionId,
                SelectedOptionId = selectedOptionId,
                IsCorrect = isCorrect,
                AnsweredAt = DateTime.UtcNow
            };
            _context.UserAnswers.Add(answer);
        }

        await _context.SaveChangesAsync();
        return MapToResponse(attempt, attempt.Exam?.Title);
    }

    public async Task<ExamAttemptResponse> SubmitExamAsync(long attemptId)
    {
        var attempt = await _context.UserAttempts
            .Include(a => a.Exam)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        if (attempt == null)
            throw new KeyNotFoundException("Attempt not found");

        if (attempt.Status == "submitted")
            throw new InvalidOperationException("Exam already submitted");

        var answers = await _context.UserAnswers
            .Where(a => a.AttemptId == attemptId)
            .ToListAsync();

        var totalQuestions = attempt.Exam?.TotalQuestions ?? answers.Count;
        var totalCorrect = answers.Count(a => a.IsCorrect == true);
        var score = totalQuestions > 0 ? Math.Round((decimal)totalCorrect / totalQuestions * 100, 2) : 0;

        attempt.Status = "submitted";
        attempt.SubmittedAt = DateTime.UtcNow;
        attempt.TotalCorrect = totalCorrect;
        attempt.TotalQuestions = totalQuestions;
        attempt.Score = score;
        attempt.TimeSpentSeconds = (int)(DateTime.UtcNow - attempt.StartedAt.GetValueOrDefault(DateTime.UtcNow)).TotalSeconds;

        await _context.SaveChangesAsync();
        return MapToResponse(attempt, attempt.Exam?.Title);
    }

    public async Task<ExamAttemptResponse?> GetAttemptByIdAsync(long attemptId)
    {
        var attempt = await _context.UserAttempts
            .Include(a => a.Exam)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        if (attempt == null) return null;
        return MapToResponse(attempt, attempt.Exam?.Title);
    }

    public async Task<List<ExamAttemptResponse>> GetUserAttemptsAsync(long userId)
    {
        var attempts = await _context.UserAttempts
            .Include(a => a.Exam)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync();

        return attempts.Select(a => MapToResponse(a, a.Exam?.Title)).ToList();
    }

    public async Task<ExamResultResponse> GetExamResultAsync(long attemptId)
    {
        var attempt = await _context.UserAttempts
            .Include(a => a.Exam)
            .Include(a => a.UserAnswers)
                .ThenInclude(ua => ua.SelectedOption)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        if (attempt == null)
            throw new KeyNotFoundException("Attempt not found");

        var questionIds = attempt.UserAnswers.Select(a => a.QuestionId).ToList();
        var questions = await _context.Questions
            .Include(q => q.QuestionOptions)
            .Where(q => questionIds.Contains(q.Id))
            .ToListAsync();

        var results = new List<QuestionResultDto>();
        foreach (var answer in attempt.UserAnswers)
        {
            var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            var correctOption = question?.QuestionOptions.FirstOrDefault(o => o.IsCorrect == true);

            results.Add(new QuestionResultDto
            {
                QuestionId = answer.QuestionId,
                Content = question?.Content ?? "",
                SelectedAnswer = answer.SelectedOption?.OptionText,
                CorrectAnswer = correctOption?.OptionText,
                IsCorrect = answer.IsCorrect ?? false
            });
        }

        return new ExamResultResponse
        {
            AttemptId = attempt.Id,
            ExamTitle = attempt.Exam?.Title ?? "",
            Score = attempt.Score ?? 0,
            TotalCorrect = attempt.TotalCorrect,
            TotalQuestions = attempt.TotalQuestions,
            StartedAt = attempt.StartedAt,
            SubmittedAt = attempt.SubmittedAt ?? DateTime.UtcNow,
            TimeSpentSeconds = attempt.TimeSpentSeconds,
            QuestionResults = results
        };
    }

    private static ExamAttemptResponse MapToResponse(UserAttempt attempt, string? examTitle)
    {
        return new ExamAttemptResponse
        {
            Id = attempt.Id,
            UserId = attempt.UserId,
            ExamId = attempt.ExamId,
            ExamTitle = examTitle,
            Score = attempt.Score,
            TotalCorrect = attempt.TotalCorrect,
            TotalQuestions = attempt.TotalQuestions,
            Status = attempt.Status ?? "in_progress",
            StartedAt = attempt.StartedAt,
            SubmittedAt = attempt.SubmittedAt,
            TimeSpentSeconds = attempt.TimeSpentSeconds
        };
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class QuestionService : IQuestionService
{
    private readonly IUnitOfWork _unitOfWork;

    public QuestionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Question>> GetAllQuestionsAsync()
    {
        return await _unitOfWork.Questions.GetAllAsync();
    }

    public async Task<Question?> GetQuestionByIdAsync(long id)
    {
        return await _unitOfWork.Questions.GetByIdAsync(id);
    }

    public async Task<List<Question>> GetQuestionsBySubjectIdAsync(int subjectId)
    {
        return await _unitOfWork.Questions.GetBySubjectIdAsync(subjectId);
    }

    public async Task<List<Question>> GetQuestionsByLessonIdAsync(int lessonId)
    {
        return await _unitOfWork.Questions.GetByLessonIdAsync(lessonId);
    }

    public async Task<Question> CreateQuestionAsync(Question question)
    {
        question.CreatedAt = DateTime.UtcNow;
        var created = await _unitOfWork.Questions.CreateAsync(question);
        await _unitOfWork.SaveChangesAsync();
        return created;
    }

    public async Task<QuestionDetailResponse?> GetQuestionDetailAsync(long id)
    {
        var question = await _unitOfWork.Questions.GetByIdAsync(id);
        if (question == null) return null;

        var uploader = question.CreatedByNavigation != null 
            ? question.CreatedByNavigation.FullName ?? question.CreatedByNavigation.Username ?? "Unknown"
            : null;

        return new QuestionDetailResponse
        {
            Id = question.Id,
            SubjectId = question.SubjectId,
            SubjectName = question.Subject?.Name,
            TopicId = question.TopicId,
            TopicName = question.Topic?.Name,
            LessonId = question.LessonId,
            QuestionType = question.QuestionType,
            Content = question.Content,
            Explanation = question.Explanation,
            Difficulty = question.Difficulty ?? 1,
            Year = question.Year,
            Source = question.Source,
            FileUrl = question.FileUrl,
            FileType = question.FileType,
            UploadedByName = uploader,
            IsActive = question.IsActive ?? true,
            Options = question.QuestionOptions.Select(o => new QuestionOptionDto
            {
                OptionKey = o.OptionKey,
                OptionText = o.OptionText,
                IsCorrect = o.IsCorrect
            }).ToList(),
            CreatedAt = question.CreatedAt
        };
    }

    public async Task UpdateQuestionAsync(Question question)
    {
        question.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Questions.UpdateAsync(question);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteQuestionAsync(long id)
    {
        await _unitOfWork.Questions.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }
}

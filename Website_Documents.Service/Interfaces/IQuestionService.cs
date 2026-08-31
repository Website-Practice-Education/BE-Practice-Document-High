using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;
using Website_Documents.Service.DTOs;

namespace Website_Documents.Service.Interfaces;

public interface IQuestionService
{
    Task<List<Question>> GetAllQuestionsAsync();
    Task<Question?> GetQuestionByIdAsync(long id);
    Task<List<Question>> GetQuestionsBySubjectIdAsync(int subjectId);
    Task<List<Question>> GetQuestionsByLessonIdAsync(int lessonId);
    Task<Question> CreateQuestionAsync(Question question);
    Task UpdateQuestionAsync(Question question);
    Task DeleteQuestionAsync(long id);
    Task<QuestionDetailResponse?> GetQuestionDetailAsync(long id);
}

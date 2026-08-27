using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.API.DTOs;
using Website_Documents.Repository.Models;

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

public interface IExamService
{
    Task<List<Exam>> GetAllExamsAsync();
    Task<Exam?> GetExamByIdAsync(long id);
    Task<List<Exam>> GetExamsBySubjectIdAsync(int subjectId);
    Task<Exam> CreateExamAsync(Exam exam);
    Task UpdateExamAsync(Exam exam);
    Task DeleteExamAsync(long id);
    Task<ExamDetailResponse?> GetExamDetailAsync(long id);
}

public interface ISubjectService
{
    Task<List<Subject>> GetAllSubjectsAsync();
    Task<Subject?> GetSubjectByIdAsync(int id);
    Task<Subject?> GetSubjectByCodeAsync(string code);
    Task<Subject> CreateSubjectAsync(Subject subject);
    Task UpdateSubjectAsync(Subject subject);
    Task DeleteSubjectAsync(int id);
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;
using Website_Documents.Service.DTOs;

namespace Website_Documents.Service.Interfaces;

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

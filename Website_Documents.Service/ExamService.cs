using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;
using Website_Documents.Service.DTOs;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class ExamService : IExamService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly BookstoreDbContext _context;

    public ExamService(IUnitOfWork unitOfWork, BookstoreDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<List<Exam>> GetAllExamsAsync()
    {
        return await _unitOfWork.Exams.GetAllAsync();
    }

    public async Task<Exam?> GetExamByIdAsync(long id)
    {
        return await _unitOfWork.Exams.GetByIdAsync(id);
    }

    public async Task<List<Exam>> GetExamsBySubjectIdAsync(int subjectId)
    {
        return await _unitOfWork.Exams.GetBySubjectIdAsync(subjectId);
    }

    public async Task<Exam> CreateExamAsync(Exam exam)
    {
        exam.CreatedAt = DateTime.UtcNow;
        var created = await _unitOfWork.Exams.CreateAsync(exam);
        await _unitOfWork.SaveChangesAsync();
        return created;
    }

    public async Task UpdateExamAsync(Exam exam)
    {
        exam.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Exams.UpdateAsync(exam);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteExamAsync(long id)
    {
        await _unitOfWork.Exams.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ExamDetailResponse?> GetExamDetailAsync(long id)
    {
        var exam = await _context.Exams
            .Include(e => e.Subject)
            .Include(e => e.CreatedByNavigation)
            .Include(e => e.ExamQuestions)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exam == null) return null;

        var creatorName = exam.CreatedByNavigation != null
            ? exam.CreatedByNavigation.FullName ?? exam.CreatedByNavigation.Email ?? "Unknown"
            : null;

        return new ExamDetailResponse
        {
            Id = exam.Id,
            Title = exam.Title,
            SubjectId = exam.SubjectId,
            SubjectName = exam.Subject?.Name,
            Description = exam.Description,
            DurationMinutes = exam.DurationMinutes,
            TotalQuestions = exam.TotalQuestions,
            Year = exam.Year,
            ExamType = exam.ExamType,
            IsTimed = exam.IsTimed,
            AllowPause = exam.AllowPause,
            ShowTimer = exam.ShowTimer,
            IsPublic = exam.IsPublic,
            CreatedByName = creatorName,
            CreatedAt = exam.CreatedAt
        };
    }
}

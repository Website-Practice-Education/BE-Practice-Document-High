using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class ExamService : IExamService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExamService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
}

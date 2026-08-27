using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class SubjectService : ISubjectService
{
    private readonly IUnitOfWork _unitOfWork;

    public SubjectService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Subject>> GetAllSubjectsAsync()
    {
        return await _unitOfWork.Subjects.GetAllAsync();
    }

    public async Task<Subject?> GetSubjectByIdAsync(int id)
    {
        return await _unitOfWork.Subjects.GetByIdAsync(id);
    }

    public async Task<Subject?> GetSubjectByCodeAsync(string code)
    {
        return await _unitOfWork.Subjects.GetByNameAsync(code);
    }

    public async Task<Subject> CreateSubjectAsync(Subject subject)
    {
        subject.CreatedAt = DateTime.UtcNow;
        var created = await _unitOfWork.Subjects.CreateAsync(subject);
        await _unitOfWork.SaveChangesAsync();
        return created;
    }

    public async Task UpdateSubjectAsync(Subject subject)
    {
        await _unitOfWork.Subjects.UpdateAsync(subject);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteSubjectAsync(int id)
    {
        await _unitOfWork.Subjects.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }
}

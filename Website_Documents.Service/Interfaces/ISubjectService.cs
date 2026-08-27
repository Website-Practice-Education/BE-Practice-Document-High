using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface ISubjectService
{
    Task<List<Subject>> GetAllSubjectsAsync();
    Task<Subject?> GetSubjectByIdAsync(int id);
    Task<Subject?> GetSubjectByCodeAsync(string code);
    Task<Subject> CreateSubjectAsync(Subject subject);
    Task UpdateSubjectAsync(Subject subject);
    Task DeleteSubjectAsync(int id);
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository.Interfaces;

public interface IExamRepository
{
    Task<List<Exam>> GetAllAsync();
    Task<Exam?> GetByIdAsync(long id);
    Task<List<Exam>> GetBySubjectIdAsync(int subjectId);
    Task<List<Exam>> GetByUserIdAsync(long userId);
    Task<Exam> CreateAsync(Exam exam);
    Task UpdateAsync(Exam exam);
    Task DeleteAsync(long id);
}

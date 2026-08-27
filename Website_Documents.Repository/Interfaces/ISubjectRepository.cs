using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository.Interfaces;

public interface ISubjectRepository
{
    Task<List<Subject>> GetAllAsync();
    Task<Subject?> GetByIdAsync(int id);
    Task<Subject?> GetByNameAsync(string code);
    Task<Subject> CreateAsync(Subject subject);
    Task UpdateAsync(Subject subject);
    Task DeleteAsync(int id);
}

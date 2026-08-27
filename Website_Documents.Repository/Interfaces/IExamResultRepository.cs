using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository.Interfaces;

public interface IExamResultRepository
{
    Task<List<UserAttempt>> GetAllAsync();
    Task<UserAttempt?> GetByIdAsync(long id);
    Task<List<UserAttempt>> GetByUserIdAsync(long userId);
    Task<List<UserAttempt>> GetByExamIdAsync(long examId);
    Task<UserAttempt> CreateAsync(UserAttempt attempt);
    Task UpdateAsync(UserAttempt attempt);
}

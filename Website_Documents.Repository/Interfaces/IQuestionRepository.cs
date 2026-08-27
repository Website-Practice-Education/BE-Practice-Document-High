using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository.Interfaces;

public interface IQuestionRepository
{
    Task<List<Question>> GetAllAsync();
    Task<List<Question>> GetAllWithDetailsAsync();
    Task<Question?> GetByIdAsync(long id);
    Task<List<Question>> GetByLessonIdAsync(int lessonId);
    Task<List<Question>> GetBySubjectIdAsync(int subjectId);
    Task<Question> CreateAsync(Question question);
    Task UpdateAsync(Question question);
    Task DeleteAsync(long id);
}

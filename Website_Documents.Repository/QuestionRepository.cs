using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository;

public class QuestionRepository : IQuestionRepository
{
    private readonly BookstoreDbContext _context;

    public QuestionRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Question>> GetAllAsync()
    {
        return await _context.Questions.ToListAsync();
    }

    public async Task<List<Question>> GetAllWithDetailsAsync()
    {
        return await _context.Questions
            .Include(q => q.Subject)
            .Include(q => q.Topic)
            .Include(q => q.Lesson)
            .Include(q => q.CreatedByNavigation)
            .ToListAsync();
    }

    public async Task<Question?> GetByIdAsync(long id)
    {
        return await _context.Questions
            .Include(q => q.Subject)
            .Include(q => q.Topic)
            .Include(q => q.Lesson)
            .Include(q => q.CreatedByNavigation)
            .Include(q => q.QuestionOptions)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<List<Question>> GetByLessonIdAsync(int lessonId)
    {
        return await _context.Questions.Where(q => q.LessonId == lessonId).ToListAsync();
    }

    public async Task<List<Question>> GetBySubjectIdAsync(int subjectId)
    {
        return await _context.Questions.Where(q => q.SubjectId == subjectId).ToListAsync();
    }

    public async Task<Question> CreateAsync(Question question)
    {
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
        return question;
    }

    public async Task UpdateAsync(Question question)
    {
        _context.Questions.Update(question);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question != null)
        {
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
        }
    }
}

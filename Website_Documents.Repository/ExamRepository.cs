using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository;

public class ExamRepository : IExamRepository
{
    private readonly BookstoreDbContext _context;

    public ExamRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Exam>> GetAllAsync()
    {
        return await _context.Exams.ToListAsync();
    }

    public async Task<Exam?> GetByIdAsync(long id)
    {
        return await _context.Exams.FindAsync(id);
    }

    public async Task<List<Exam>> GetBySubjectIdAsync(int subjectId)
    {
        return await _context.Exams.Where(e => e.SubjectId == subjectId).ToListAsync();
    }

    public async Task<List<Exam>> GetByUserIdAsync(long userId)
    {
        return await _context.Exams.Where(e => e.CreatedBy == userId).ToListAsync();
    }

    public async Task<Exam> CreateAsync(Exam exam)
    {
        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();
        return exam;
    }

    public async Task UpdateAsync(Exam exam)
    {
        _context.Exams.Update(exam);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam != null)
        {
            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();
        }
    }
}

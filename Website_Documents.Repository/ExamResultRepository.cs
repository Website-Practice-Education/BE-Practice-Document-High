using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository;

public class ExamResultRepository : IExamResultRepository
{
    private readonly BookstoreDbContext _context;

    public ExamResultRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserAttempt>> GetAllAsync()
    {
        return await _context.UserAttempts.ToListAsync();
    }

    public async Task<UserAttempt?> GetByIdAsync(long id)
    {
        return await _context.UserAttempts.FindAsync(id);
    }

    public async Task<List<UserAttempt>> GetByUserIdAsync(long userId)
    {
        return await _context.UserAttempts.Where(a => a.UserId == userId).ToListAsync();
    }

    public async Task<List<UserAttempt>> GetByExamIdAsync(long examId)
    {
        return await _context.UserAttempts.Where(a => a.ExamId == examId).ToListAsync();
    }

    public async Task<UserAttempt> CreateAsync(UserAttempt attempt)
    {
        _context.UserAttempts.Add(attempt);
        await _context.SaveChangesAsync();
        return attempt;
    }

    public async Task UpdateAsync(UserAttempt attempt)
    {
        _context.UserAttempts.Update(attempt);
        await _context.SaveChangesAsync();
    }
}

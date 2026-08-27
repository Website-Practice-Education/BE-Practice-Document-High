using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository;

public class SubjectRepository : ISubjectRepository
{
    private readonly BookstoreDbContext _context;

    public SubjectRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Subject>> GetAllAsync()
    {
        return await _context.Subjects.ToListAsync();
    }

    public async Task<Subject?> GetByIdAsync(int id)
    {
        return await _context.Subjects.FindAsync(id);
    }

    public async Task<Subject?> GetByNameAsync(string code)
    {
        return await _context.Subjects.FirstOrDefaultAsync(s => s.Code == code);
    }

    public async Task<Subject> CreateAsync(Subject subject)
    {
        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();
        return subject;
    }

    public async Task UpdateAsync(Subject subject)
    {
        _context.Subjects.Update(subject);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject != null)
        {
            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
        }
    }
}

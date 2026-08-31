using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly BookstoreDbContext _context;

    public PasswordResetTokenRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        return await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token && 
                                       !t.IsUsed && 
                                       t.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<PasswordResetToken> CreateAsync(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }

    public async Task UpdateAsync(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    public async Task InvalidateUserTokensAsync(long userId)
    {
        var tokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == userId && !t.IsUsed)
            .ToListAsync();
        
        foreach (var token in tokens)
        {
            token.IsUsed = true;
        }
        
        await _context.SaveChangesAsync();
    }
}

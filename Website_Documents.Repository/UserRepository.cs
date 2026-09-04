using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository;

public class UserRepository : IUserRepository
{
    private readonly BookstoreDbContext _context;

    public UserRepository(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllAsync()
    {
        var count = await _context.Users.CountAsync();
        Console.WriteLine($"[DEBUG UserRepository] Total users in DB: {count}");
        var users = await _context.Users.ToListAsync();
        Console.WriteLine($"[DEBUG UserRepository] GetAllAsync returned {users.Count} users");
        return users;
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.FullName == username);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<User>> GetUsersByRoleAsync(string role)
    {
        return await _context.Users.Where(u => u.Role == role).ToListAsync();
    }

    public async Task<List<User>> SearchUsersAsync(string? keyword, string? role, bool? isActive)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(lowerKeyword)) ||
                (u.FullName != null && u.FullName.ToLower().Contains(lowerKeyword)));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => u.Role == role);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<List<User>> GetRecentlyActiveAsync(int count, int daysSinceLastLogin)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysSinceLastLogin);
        return await _context.Users
            .Where(u => u.LastLoginAt != null && u.LastLoginAt >= cutoffDate)
            .OrderByDescending(u => u.LastLoginAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<User>> GetInactiveUsersAsync(int inactiveDays)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-inactiveDays);
        return await _context.Users
            .Where(u => u.LastLoginAt == null || u.LastLoginAt < cutoffDate)
            .ToListAsync();
    }

    public async Task<int> DeleteMultipleAsync(List<long> userIds)
    {
        var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
        if (users.Count == 0) return 0;

        _context.Users.RemoveRange(users);
        await _context.SaveChangesAsync();
        return users.Count;
    }

    public async Task<int> DeactivateInactiveAsync(int inactiveDays)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-inactiveDays);
        var inactiveUsers = await _context.Users
            .Where(u => (u.LastLoginAt == null || u.LastLoginAt < cutoffDate) && u.IsActive == true)
            .ToListAsync();

        if (inactiveUsers.Count == 0) return 0;

        foreach (var user in inactiveUsers)
        {
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return inactiveUsers.Count;
    }
}

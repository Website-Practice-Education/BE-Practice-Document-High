using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Repository.Interfaces;

public interface IUserRepository
{
    // Basic CRUD
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(long id);

    // Role Management
    Task<List<User>> GetUsersByRoleAsync(string role);

    // Search & Filtering
    Task<List<User>> SearchUsersAsync(string? keyword, string? role, bool? isActive);
    Task<int> GetTotalCountAsync();
    Task<List<User>> GetRecentlyActiveAsync(int count, int daysSinceLastLogin);
    Task<List<User>> GetInactiveUsersAsync(int inactiveDays);

    // Batch Operations
    Task<int> DeleteMultipleAsync(List<long> userIds);
    Task<int> DeactivateInactiveAsync(int inactiveDays);
}

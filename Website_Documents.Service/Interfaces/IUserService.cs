using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface IUserService
{
    // Basic CRUD
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(long id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(long id);

    // Authentication & Security
    Task<bool> ValidateCredentialsAsync(string email, string password);
    Task<bool> ChangePasswordAsync(long userId, string currentPassword, string newPassword);
    Task<string?> GenerateResetTokenAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);

    // Role Management
    Task<List<User>> GetUsersByRoleAsync(string role);
    Task<bool> UpdateUserRoleAsync(long userId, string role);

    // Account Status
    Task<bool> UpdateUserStatusAsync(long userId, bool isActive);
    Task<bool> LockUserAccountAsync(long userId, DateTime lockUntil);
    Task<bool> UnlockUserAccountAsync(long userId);

    // Search & Filtering
    Task<List<User>> SearchUsersAsync(string? keyword, string? role, bool? isActive);
    Task<int> GetTotalUsersCountAsync();
    Task<List<User>> GetRecentlyActiveUsersAsync(int count);
    Task<List<User>> GetInactiveUsersAsync(int daysSinceLastLogin);

    // Profile Management
    Task<bool> UpdateProfilePictureAsync(long userId, string imageUrl);
    Task UpdateLastLoginAsync(long userId);
    Task<User?> UpdateProfileAsync(long userId, string? fullName, short? grade, string? avatarUrl);

    // Batch Operations
    Task<int> DeleteMultipleUsersAsync(List<long> userIds);
    Task<int> DeactivateInactiveUsersAsync(int inactiveDays);
}

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Website_Documents.Repository;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ===== Basic CRUD =====

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _unitOfWork.Users.GetAllAsync();
    }

    public async Task<User?> GetUserByIdAsync(long id)
    {
        return await _unitOfWork.Users.GetByIdAsync(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _unitOfWork.Users.GetByEmailAsync(email);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        var created = await _unitOfWork.Users.CreateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return created;
    }

    public async Task UpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(long id)
    {
        await _unitOfWork.Users.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    // ===== Authentication & Security =====

    public async Task<bool> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        if (user == null) return false;
        return VerifyPassword(password, user.PasswordHash);
    }

    public async Task<bool> ChangePasswordAsync(long userId, string currentPassword, string newPassword)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        if (!VerifyPassword(currentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect");

        user.PasswordHash = HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<string?> GenerateResetTokenAsync(string email)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        if (user == null) return null;

        // Invalidate any existing tokens for this user
        await _unitOfWork.PasswordResetTokens.InvalidateUserTokensAsync(user.Id);

        // Generate a secure reset token
        var tokenBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        var token = Convert.ToBase64String(tokenBytes);

        // Store token in database with expiration (24 hours)
        var resetToken = new Repository.Models.PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.PasswordResetTokens.CreateAsync(resetToken);
        return token;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
            return false;

        // Find valid token
        var resetToken = await _unitOfWork.PasswordResetTokens.GetByTokenAsync(token);
        if (resetToken == null)
            return false;

        // Get user
        var user = await _unitOfWork.Users.GetByIdAsync(resetToken.UserId);
        if (user == null)
            return false;

        // Update password
        user.PasswordHash = HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);

        // Mark token as used
        resetToken.IsUsed = true;
        await _unitOfWork.PasswordResetTokens.UpdateAsync(resetToken);

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // ===== Role Management =====

    public async Task<List<User>> GetUsersByRoleAsync(string role)
    {
        return await _unitOfWork.Users.GetUsersByRoleAsync(role);
    }

    public async Task<bool> UpdateUserRoleAsync(long userId, string role)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // ===== Account Status =====

    public async Task<bool> UpdateUserStatusAsync(long userId, bool isActive)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> LockUserAccountAsync(long userId, DateTime lockUntil)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // In production, store lockUntil in a separate field or lockout table
        return true;
    }

    public async Task<bool> UnlockUserAccountAsync(long userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // ===== Search & Filtering =====

    public async Task<List<User>> SearchUsersAsync(string? keyword, string? role, bool? isActive)
    {
        return await _unitOfWork.Users.SearchUsersAsync(keyword, role, isActive);
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _unitOfWork.Users.GetTotalCountAsync();
    }

    public async Task<List<User>> GetRecentlyActiveUsersAsync(int count)
    {
        return await _unitOfWork.Users.GetRecentlyActiveAsync(count, 30); // Within last 30 days
    }

    public async Task<List<User>> GetInactiveUsersAsync(int daysSinceLastLogin)
    {
        return await _unitOfWork.Users.GetInactiveUsersAsync(daysSinceLastLogin);
    }

    // ===== Profile Management =====

    public async Task<bool> UpdateProfilePictureAsync(long userId, string imageUrl)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        user.AvatarUrl = imageUrl;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task UpdateLastLoginAsync(long userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<User?> UpdateProfileAsync(long userId, string? fullName, short? grade, string? avatarUrl)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return null;

        if (!string.IsNullOrEmpty(fullName))
            user.FullName = fullName;
        if (grade.HasValue)
            user.Grade = grade;
        if (!string.IsNullOrEmpty(avatarUrl))
            user.AvatarUrl = avatarUrl;

        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return user;
    }

    // ===== Batch Operations =====

    public async Task<int> DeleteMultipleUsersAsync(List<long> userIds)
    {
        return await _unitOfWork.Users.DeleteMultipleAsync(userIds);
    }

    public async Task<int> DeactivateInactiveUsersAsync(int inactiveDays)
    {
        return await _unitOfWork.Users.DeactivateInactiveAsync(inactiveDays);
    }

    // ===== Private Helper Methods =====

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}

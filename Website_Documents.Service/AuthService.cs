using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Website_Documents.API.DTOs;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _config = config;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password");

        if (!VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddDays(7);

        return new LoginResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName ?? "",
            Role = user.Role ?? "student",
            ExpiresAt = expiresAt
        };
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email already exists");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            FullName = request.FullName,
            Role = "student",
            Grade = request.Grade,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _unitOfWork.Users.CreateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var token = GenerateJwtToken(createdUser);
        var expiresAt = DateTime.UtcNow.AddDays(7);

        return new LoginResponse
        {
            Token = token,
            Email = createdUser.Email,
            FullName = createdUser.FullName ?? "",
            Role = createdUser.Role ?? "student",
            ExpiresAt = expiresAt
        };
    }

    public async Task<bool> ChangePasswordAsync(long userId, ChangePasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect");

        user.PasswordHash = HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateProfileAsync(long userId, UpdateProfileRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        if (!string.IsNullOrEmpty(request.FullName))
            user.FullName = request.FullName;
        if (request.Grade.HasValue)
            user.Grade = request.Grade;
        if (!string.IsNullOrEmpty(request.AvatarUrl))
            user.AvatarUrl = request.AvatarUrl;

        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName ?? ""),
            new Claim(ClaimTypes.Role, user.Role ?? "student")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}

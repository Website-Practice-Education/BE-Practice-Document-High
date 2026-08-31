using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;
using Website_Documents.Service.DTOs;
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

    public async Task<LoginResponse> GoogleLoginAsync(string googleToken)
    {
        // Validate Google token and get user info
        var googleUserInfo = await ValidateGoogleTokenAsync(googleToken);
        
        // Check if user exists by email
        var user = await _unitOfWork.Users.GetByEmailAsync(googleUserInfo.Email);
        
        if (user == null)
        {
            // Create new user from Google account
            user = new User
            {
                Email = googleUserInfo.Email,
                FullName = googleUserInfo.Name,
                AvatarUrl = googleUserInfo.Picture,
                PasswordHash = "", // No password for Google users
                Role = "student",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            user = await _unitOfWork.Users.CreateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
        else
        {
            // Update last login and avatar if changed
            user.LastLoginAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(googleUserInfo.Picture))
                user.AvatarUrl = googleUserInfo.Picture;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

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

    private async Task<GoogleUserInfo> ValidateGoogleTokenAsync(string token)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config["Google:ClientId"] }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);

            return new GoogleUserInfo
            {
                Email = payload.Email,
                Name = payload.Name ?? payload.GivenName ?? "User",
                Picture = payload.Picture ?? ""
            };
        }
        catch (Exception)
        {
            throw new UnauthorizedAccessException("Invalid Google token");
        }
    }

    private class GoogleUserInfo
    {
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
        public string Picture { get; set; } = "";
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

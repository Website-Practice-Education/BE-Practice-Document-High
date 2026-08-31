using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Service.DTOs;

namespace Website_Documents.Service.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> GoogleLoginAsync(string googleToken);
    Task<bool> ChangePasswordAsync(long userId, ChangePasswordRequest request);
    Task<bool> UpdateProfileAsync(long userId, UpdateProfileRequest request);
}

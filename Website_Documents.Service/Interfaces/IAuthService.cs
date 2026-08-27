using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.API.DTOs;

namespace Website_Documents.Service.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<bool> ChangePasswordAsync(long userId, ChangePasswordRequest request);
    Task<bool> UpdateProfileAsync(long userId, UpdateProfileRequest request);
}

using System;
using System.ComponentModel.DataAnnotations;

namespace Website_Documents.API.DTOs;

// ===== Request DTOs =====

public class UpdateRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}

public class UpdateStatusRequest
{
    public bool IsActive { get; set; }
}

public class LockAccountRequest
{
    [Required]
    public DateTime LockUntil { get; set; }
}

public class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}

public class DeleteMultipleUsersRequest
{
    [Required]
    public List<long> UserIds { get; set; } = new();
}

public class SearchUsersRequest
{
    public string? Keyword { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// ===== Response DTOs =====

public class UserResponse
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public short? Grade { get; set; }
    public string? AvatarUrl { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class UserSearchResponse
{
    public List<UserResponse> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class ResetPasswordResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class PasswordChangeResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class BatchOperationResponse
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<long> FailedIds { get; set; } = new();
}

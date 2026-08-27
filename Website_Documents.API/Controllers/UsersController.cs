using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.API.DTOs;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // ===== Basic CRUD =====

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound(new { message = "User not found" });
        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        if (user == null) return NotFound(new { message = "User not found" });
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] User user)
    {
        var created = await _userService.CreateUserAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] User user)
    {
        user.Id = id;
        await _userService.UpdateUserAsync(user);
        return Ok(new { message = "User updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok(new { message = "User deleted successfully" });
    }

    // ===== Authentication & Security =====

    [HttpPost("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(long id, [FromBody] ChangePasswordRequest request)
    {
        try
        {
            var result = await _userService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);
            if (!result) return NotFound(new { message = "User not found" });

            return Ok(new PasswordChangeResponse
            {
                Success = true,
                Message = "Password changed successfully"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new PasswordChangeResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var token = await _userService.GenerateResetTokenAsync(request.Email);
        if (token == null)
            return NotFound(new { message = "User with this email not found" });

        // In production, send email with reset link
        return Ok(new { message = "Password reset token generated", token });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _userService.ResetPasswordAsync(request.Token, request.NewPassword);
        if (!result)
            return BadRequest(new { message = "Invalid or expired token" });

        return Ok(new ResetPasswordResponse
        {
            Success = true,
            Message = "Password reset successfully"
        });
    }

    // ===== Role Management =====

    [HttpGet("role/{role}")]
    public async Task<IActionResult> GetByRole(string role)
    {
        var users = await _userService.GetUsersByRoleAsync(role);
        return Ok(users);
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(long id, [FromBody] UpdateRoleRequest request)
    {
        var result = await _userService.UpdateUserRoleAsync(id, request.Role);
        if (!result) return NotFound(new { message = "User not found" });

        return Ok(new { message = "User role updated successfully" });
    }

    // ===== Account Status =====

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateStatusRequest request)
    {
        var result = await _userService.UpdateUserStatusAsync(id, request.IsActive);
        if (!result) return NotFound(new { message = "User not found" });

        return Ok(new { message = "User status updated successfully" });
    }

    [HttpPost("{id}/lock")]
    public async Task<IActionResult> LockAccount(long id, [FromBody] LockAccountRequest request)
    {
        var result = await _userService.LockUserAccountAsync(id, request.LockUntil);
        if (!result) return NotFound(new { message = "User not found" });

        return Ok(new { message = "User account locked successfully", lockUntil = request.LockUntil });
    }

    [HttpPost("{id}/unlock")]
    public async Task<IActionResult> UnlockAccount(long id)
    {
        var result = await _userService.UnlockUserAccountAsync(id);
        if (!result) return NotFound(new { message = "User not found" });

        return Ok(new { message = "User account unlocked successfully" });
    }

    // ===== Search & Filtering =====

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? keyword,
        [FromQuery] string? role,
        [FromQuery] bool? isActive)
    {
        var users = await _userService.SearchUsersAsync(keyword, role, isActive);
        return Ok(users);
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        var count = await _userService.GetTotalUsersCountAsync();
        return Ok(new { totalUsers = count });
    }

    [HttpGet("recently-active")]
    public async Task<IActionResult> GetRecentlyActive([FromQuery] int count = 10)
    {
        var users = await _userService.GetRecentlyActiveUsersAsync(count);
        return Ok(users);
    }

    [HttpGet("inactive")]
    public async Task<IActionResult> GetInactive([FromQuery] int days = 90)
    {
        var users = await _userService.GetInactiveUsersAsync(days);
        return Ok(users);
    }

    // ===== Profile Management =====

    [HttpPut("{id}/avatar")]
    public async Task<IActionResult> UpdateAvatar(long id, [FromBody] UpdateAvatarRequest request)
    {
        var result = await _userService.UpdateProfilePictureAsync(id, request.AvatarUrl);
        if (!result) return NotFound(new { message = "User not found" });

        return Ok(new { message = "Avatar updated successfully" });
    }

    [HttpPut("{id}/profile")]
    public async Task<IActionResult> UpdateProfile(long id, [FromBody] UpdateProfileRequest request)
    {
        var user = await _userService.UpdateProfileAsync(id, request.FullName, request.Grade, request.AvatarUrl);
        if (user == null) return NotFound(new { message = "User not found" });

        return Ok(user);
    }

    [HttpPost("{id}/login")]
    public async Task<IActionResult> RecordLogin(long id)
    {
        await _userService.UpdateLastLoginAsync(id);
        return Ok(new { message = "Last login updated" });
    }

    // ===== Batch Operations =====

    [HttpPost("delete-multiple")]
    public async Task<IActionResult> DeleteMultiple([FromBody] DeleteMultipleUsersRequest request)
    {
        var deletedCount = await _userService.DeleteMultipleUsersAsync(request.UserIds);
        return Ok(new BatchOperationResponse
        {
            SuccessCount = deletedCount,
            FailedCount = request.UserIds.Count - deletedCount,
            FailedIds = new List<long>()
        });
    }

    [HttpPost("deactivate-inactive")]
    public async Task<IActionResult> DeactivateInactive([FromQuery] int inactiveDays = 90)
    {
        var deactivatedCount = await _userService.DeactivateInactiveUsersAsync(inactiveDays);
        return Ok(new { message = $"Deactivated {deactivatedCount} inactive users" });
    }
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class UpdateAvatarRequest
{
    public string AvatarUrl { get; set; } = string.Empty;
}

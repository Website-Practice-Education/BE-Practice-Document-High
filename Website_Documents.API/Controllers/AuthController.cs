using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Website_Documents.API.DTOs;
using Website_Documents.Service.DTOs;
using Website_Documents.Service.Interfaces;
using ApiDTOs = Website_Documents.API.DTOs;
using ServiceDTOs = Website_Documents.Service.DTOs;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] ServiceDTOs.RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(ApiResponse<ServiceDTOs.LoginResponse>.SuccessResponse(result, "Registration successful"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] ServiceDTOs.LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResponse<ServiceDTOs.LoginResponse>.SuccessResponse(result, "Login successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] ApiDTOs.GoogleLoginRequest request)
    {
        try
        {
            var result = await _authService.GoogleLoginAsync(request.Token);
            return Ok(ApiResponse<ServiceDTOs.LoginResponse>.SuccessResponse(result, "Google login successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ServiceDTOs.ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        try
        {
            var result = await _authService.ChangePasswordAsync(userId.Value, request);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Password changed successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] ServiceDTOs.UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var result = await _authService.UpdateProfileAsync(userId.Value, request);
        if (!result)
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Profile updated successfully"));
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}

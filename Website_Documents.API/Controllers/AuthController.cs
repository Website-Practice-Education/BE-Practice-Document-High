using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Website_Documents.Repository.Models;
using Website_Documents.Repository.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthController(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Check if user already exists
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { success = false, message = "Email đã được sử dụng" });
        }

        // Create new user
        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName ?? request.Email.Split('@')[0],
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Grade = (short)(request.Grade ?? 10),
            Role = "Student",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _unitOfWork.Users.CreateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            success = true,
            message = "Đăng ký thành công",
            data = new
            {
                token = token,
                email = user.Email,
                fullName = user.FullName,
                role = user.Role
            }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { success = false, message = "Email hoặc mật khẩu không đúng" });
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            success = true,
            message = "Đăng nhập thành công",
            data = new
            {
                token = token,
                email = user.Email,
                fullName = user.FullName,
                role = user.Role
            }
        });
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            // Verify Google token
            var googleApiUrl = $"https://oauth2.googleapis.com/tokeninfo?id_token={request.Token}";
            using var client = new HttpClient();
            var response = await client.GetAsync(googleApiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return Unauthorized(new { success = false, message = "Token Google không hợp lệ" });
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (tokenInfo == null)
            {
                return Unauthorized(new { success = false, message = "Không thể xác thực Google token" });
            }

            // Find or create user
            var user = await _unitOfWork.Users.GetByEmailAsync(tokenInfo.Email);

            if (user == null)
            {
                // Create new user from Google info
                user = new User
                {
                    Email = tokenInfo.Email,
                    FullName = tokenInfo.Name ?? tokenInfo.Email.Split('@')[0],
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    Grade = 10,
                    Role = "Student",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    AvatarUrl = tokenInfo.Picture
                };

                await _unitOfWork.Users.CreateAsync(user);
                await _unitOfWork.SaveChangesAsync();
            }

            var jwtToken = GenerateJwtToken(user);

            return Ok(new
            {
                success = true,
                message = "Đăng nhập Google thành công",
                data = new
                {
                    token = jwtToken,
                    email = user.Email,
                    fullName = user.FullName,
                    role = user.Role
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
        }
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSecret = _configuration["Jwt:Secret"] ?? "DefaultSecretKeyForJwtToken12345678901234567890";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName ?? ""),
            new Claim(ClaimTypes.Role, user.Role ?? "Student")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "WebsiteDocuments",
            audience: _configuration["Jwt:Audience"] ?? "WebsiteDocuments",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public int? Grade { get; set; }
}

public class GoogleLoginRequest
{
    public string Token { get; set; } = string.Empty;
}

public class GoogleTokenInfo
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Picture { get; set; }
}

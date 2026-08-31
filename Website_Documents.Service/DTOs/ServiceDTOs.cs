namespace Website_Documents.Service.DTOs;

// ===== Auth DTOs =====
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public short? Grade { get; set; }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    public string? FullName { get; set; }
    public short? Grade { get; set; }
    public string? AvatarUrl { get; set; }
}

// ===== Exam DTOs =====
public class ExamDetailResponse
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public int? TotalQuestions { get; set; }
    public short? Year { get; set; }
    public string? ExamType { get; set; }
    public bool? IsTimed { get; set; }
    public bool? AllowPause { get; set; }
    public bool? ShowTimer { get; set; }
    public bool? IsPublic { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? CreatedAt { get; set; }
}

// ===== Question DTOs =====
public class QuestionDetailResponse
{
    public long Id { get; set; }
    public int SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public int? TopicId { get; set; }
    public string? TopicName { get; set; }
    public int? LessonId { get; set; }
    public string? QuestionType { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public int Difficulty { get; set; }
    public short? Year { get; set; }
    public string? Source { get; set; }
    public string? FileUrl { get; set; }
    public string? FileType { get; set; }
    public string? UploadedByName { get; set; }
    public bool IsActive { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class QuestionOptionDto
{
    public string OptionKey { get; set; } = string.Empty;
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

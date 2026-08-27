using System.ComponentModel.DataAnnotations;

namespace Website_Documents.API.DTOs;

public class CreateExamRequest
{
    [Required]
    [MinLength(3)]
    public string Title { get; set; } = string.Empty;

    public int SubjectId { get; set; }

    public string? Description { get; set; }

    [Range(1, 300)]
    public int DurationMinutes { get; set; } = 60;

    public short? Year { get; set; }

    public string? ExamType { get; set; }

    public bool IsTimed { get; set; } = true;
    public bool AllowPause { get; set; } = false;
    public bool ShowTimer { get; set; } = true;
    public bool IsPublic { get; set; } = false;
}

public class UpdateExamRequest
{
    public string? Title { get; set; }
    public int? SubjectId { get; set; }
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public short? Year { get; set; }
    public string? ExamType { get; set; }
    public bool? IsTimed { get; set; }
    public bool? AllowPause { get; set; }
    public bool? ShowTimer { get; set; }
    public bool? IsPublic { get; set; }
}

public class ExamDetailResponse
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public int TotalQuestions { get; set; }
    public short? Year { get; set; }
    public string? ExamType { get; set; }
    public bool IsTimed { get; set; }
    public bool AllowPause { get; set; }
    public bool ShowTimer { get; set; }
    public bool IsPublic { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? CreatedAt { get; set; }
}

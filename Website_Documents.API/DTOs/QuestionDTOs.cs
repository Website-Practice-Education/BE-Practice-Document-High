using System.ComponentModel.DataAnnotations;

namespace Website_Documents.API.DTOs;

public class CreateQuestionRequest
{
    [Required]
    public int SubjectId { get; set; }

    public int? TopicId { get; set; }
    public int? LessonId { get; set; }

    [Required]
    public string QuestionType { get; set; } = "multiple_choice";

    [Required]
    [MinLength(10)]
    public string Content { get; set; } = string.Empty;

    public string? Explanation { get; set; }

    [Range(1, 5)]
    public short Difficulty { get; set; } = 1;

    public short? Year { get; set; }
    public string? Source { get; set; }
    public string? FileUrl { get; set; }
    public string? FileType { get; set; }
    public string? UploadedByName { get; set; }

    [Required]
    [MinLength(2)]
    public List<QuestionOptionDto> Options { get; set; } = new();
}

public class QuestionOptionDto
{
    [Required]
    public string OptionKey { get; set; } = string.Empty;

    [Required]
    public string OptionText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; } = false;
}

public class QuestionDetailResponse
{
    public long Id { get; set; }
    public int SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public int? TopicId { get; set; }
    public string? TopicName { get; set; }
    public int? LessonId { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public short Difficulty { get; set; }
    public short? Year { get; set; }
    public string? Source { get; set; }
    public string? FileUrl { get; set; }
    public string? FileType { get; set; }
    public string? UploadedByName { get; set; }
    public bool IsActive { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = new();
    public DateTime? CreatedAt { get; set; }
}

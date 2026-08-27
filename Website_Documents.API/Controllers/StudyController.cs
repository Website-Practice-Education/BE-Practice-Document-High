using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.API.DTOs;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudyController : ControllerBase
{
    private readonly IStudyService _studyService;
    private readonly IProgressService _progressService;

    public StudyController(IStudyService studyService, IProgressService progressService)
    {
        _studyService = studyService;
        _progressService = progressService;
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    // ===== Study Session =====

    [HttpPost("session/start")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var session = await _studyService.StartStudySessionAsync(userId.Value, request.SubjectId);
        return Ok(ApiResponse<object>.SuccessResponse(session, "Study session started"));
    }

    [HttpGet("session/{sessionId}")]
    public async Task<IActionResult> GetSession(long sessionId)
    {
        var session = await _studyService.GetStudySessionAsync(sessionId);
        if (session == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Session not found"));

        return Ok(ApiResponse<object>.SuccessResponse(session));
    }

    [HttpGet("session/active")]
    public async Task<IActionResult> GetActiveSession()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var session = await _studyService.GetActiveSessionAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(session));
    }

    [HttpPost("session/{sessionId}/end")]
    public async Task<IActionResult> EndSession(long sessionId)
    {
        var session = await _studyService.EndStudySessionAsync(sessionId);
        return Ok(ApiResponse<object>.SuccessResponse(session, "Session ended"));
    }

    [HttpPost("session/{sessionId}/progress")]
    public async Task<IActionResult> UpdateProgress(long sessionId, [FromBody] UpdateProgressRequest request)
    {
        await _studyService.UpdateStudySessionProgressAsync(sessionId, request.QuestionsAnswered, request.CorrectAnswers, request.TimeSpentMinutes);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Progress updated"));
    }

    // ===== Practice Questions =====

    [HttpGet("practice")]
    public async Task<IActionResult> GetPracticeQuestions(
        [FromQuery] int subjectId,
        [FromQuery] int topicId = 0,
        [FromQuery] int count = 10,
        [FromQuery] short? minDifficulty = null,
        [FromQuery] short? maxDifficulty = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var questions = await _studyService.GetPracticeQuestionsAsync(userId.Value, subjectId, topicId, count, minDifficulty, maxDifficulty);
        return Ok(ApiResponse<object>.SuccessResponse(questions));
    }

    [HttpGet("practice/weak")]
    public async Task<IActionResult> GetWeakQuestions([FromQuery] int count = 10)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var questions = await _studyService.GetWeakQuestionsAsync(userId.Value, count);
        return Ok(ApiResponse<object>.SuccessResponse(questions));
    }

    [HttpGet("practice/recommended")]
    public async Task<IActionResult> GetRecommendedQuestions([FromQuery] int count = 10)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var questions = await _studyService.GetRecommendedQuestionsAsync(userId.Value, count);
        return Ok(ApiResponse<object>.SuccessResponse(questions));
    }

    // ===== Quiz Mode =====

    [HttpPost("quiz/start")]
    public async Task<IActionResult> StartQuiz([FromBody] StartQuizRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var quiz = await _studyService.StartQuizAsync(userId.Value, request.SubjectId, request.QuestionCount, request.Difficulty);
        return Ok(ApiResponse<object>.SuccessResponse(quiz, "Quiz started"));
    }

    [HttpPost("quiz/{sessionId}/answer")]
    public async Task<IActionResult> SubmitAnswer(long sessionId, [FromBody] SubmitAnswerRequest request)
    {
        await _studyService.SubmitQuizAnswerAsync(sessionId, request.QuestionId, request.SelectedOptionId, request.IsCorrect, request.TimeSpentSeconds);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Answer submitted"));
    }

    [HttpPost("quiz/{sessionId}/complete")]
    public async Task<IActionResult> CompleteQuiz(long sessionId)
    {
        var result = await _studyService.CompleteQuizAsync(sessionId);
        return Ok(ApiResponse<object>.SuccessResponse(result, "Quiz completed"));
    }

    // ===== Statistics =====

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var stats = await _studyService.GetStudyStatisticsAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(stats));
    }

    [HttpGet("topic-mastery")]
    public async Task<IActionResult> GetTopicMastery()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var mastery = await _studyService.GetTopicMasteryAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(mastery));
    }

    [HttpGet("subject/{subjectId}/progress")]
    public async Task<IActionResult> GetSubjectProgress(int subjectId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var progress = await _studyService.GetSubjectProgressAsync(userId.Value, subjectId);
        return Ok(ApiResponse<object>.SuccessResponse(progress));
    }

    // ===== Dashboard =====

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var dashboard = await _progressService.GetDashboardAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(dashboard));
    }
}

public class StartSessionRequest
{
    public int SubjectId { get; set; }
}

public class UpdateProgressRequest
{
    public int QuestionsAnswered { get; set; }
    public int CorrectAnswers { get; set; }
    public int TimeSpentMinutes { get; set; }
}

public class StartQuizRequest
{
    public int SubjectId { get; set; }
    public int QuestionCount { get; set; }
    public string Difficulty { get; set; } = "medium";
}

public class SubmitAnswerRequest
{
    public long QuestionId { get; set; }
    public long? SelectedOptionId { get; set; }
    public bool IsCorrect { get; set; }
    public int TimeSpentSeconds { get; set; }
}

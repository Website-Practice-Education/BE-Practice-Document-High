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
public class ExamAttemptsController : ControllerBase
{
    private readonly IExamAttemptService _attemptService;

    public ExamAttemptsController(IExamAttemptService attemptService)
    {
        _attemptService = attemptService;
    }

    [HttpPost("start/{examId}")]
    public async Task<IActionResult> StartExam(long examId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var result = await _attemptService.StartExamAsync(userId.Value, examId);
        return Ok(ApiResponse<ExamAttemptResponse>.SuccessResponse(result, "Exam started"));
    }

    [HttpPost("{attemptId}/answer")]
    public async Task<IActionResult> SubmitAnswer(long attemptId, [FromBody] SubmitExamAnswerRequest request)
    {
        var result = await _attemptService.SubmitAnswerAsync(attemptId, request.QuestionId, request.SelectedOptionId);
        return Ok(ApiResponse<ExamAttemptResponse>.SuccessResponse(result, "Answer saved"));
    }

    [HttpPost("{attemptId}/submit")]
    public async Task<IActionResult> SubmitExam(long attemptId)
    {
        var result = await _attemptService.SubmitExamAsync(attemptId);
        return Ok(ApiResponse<ExamAttemptResponse>.SuccessResponse(result, "Exam submitted"));
    }

    [HttpGet("{attemptId}")]
    public async Task<IActionResult> GetAttempt(long attemptId)
    {
        var result = await _attemptService.GetAttemptByIdAsync(attemptId);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Attempt not found"));

        return Ok(ApiResponse<ExamAttemptResponse>.SuccessResponse(result));
    }

    [HttpGet("{attemptId}/result")]
    public async Task<IActionResult> GetResult(long attemptId)
    {
        var result = await _attemptService.GetExamResultAsync(attemptId);
        return Ok(ApiResponse<ExamResultResponse>.SuccessResponse(result));
    }

    [HttpGet("my-attempts")]
    public async Task<IActionResult> GetMyAttempts()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var result = await _attemptService.GetUserAttemptsAsync(userId.Value);
        return Ok(ApiResponse<List<ExamAttemptResponse>>.SuccessResponse(result));
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}

public class SubmitExamAnswerRequest
{
    public long QuestionId { get; set; }
    public long? SelectedOptionId { get; set; }
}

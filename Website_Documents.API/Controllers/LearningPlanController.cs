using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.API.DTOs;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/learning")]
[Authorize]
public class LearningPlanController : ControllerBase
{
    private readonly ILearningPlanService _learningPlanService;

    public LearningPlanController(ILearningPlanService learningPlanService)
    {
        _learningPlanService = learningPlanService;
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    // ===== Learning Plan =====

    [HttpPost("plan")]
    public async Task<IActionResult> CreatePlan([FromBody] CreateLearningPlanRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var plan = await _learningPlanService.CreateLearningPlanAsync(userId.Value, request);
        return Ok(ApiResponse<object>.SuccessResponse(plan, "Learning plan created"));
    }

    [HttpGet("plan/{planId}")]
    public async Task<IActionResult> GetPlan(long planId)
    {
        var plan = await _learningPlanService.GetLearningPlanAsync(planId);
        if (plan == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Plan not found"));

        return Ok(ApiResponse<object>.SuccessResponse(plan));
    }

    [HttpGet("plans")]
    public async Task<IActionResult> GetUserPlans([FromQuery] bool? isActive = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var plans = await _learningPlanService.GetUserLearningPlansAsync(userId.Value, isActive);
        return Ok(ApiResponse<object>.SuccessResponse(plans));
    }

    [HttpPut("plan/{planId}")]
    public async Task<IActionResult> UpdatePlan(long planId, [FromBody] UpdateLearningPlanRequest request)
    {
        var plan = await _learningPlanService.UpdateLearningPlanAsync(planId, request);
        return Ok(ApiResponse<object>.SuccessResponse(plan, "Plan updated"));
    }

    [HttpDelete("plan/{planId}")]
    public async Task<IActionResult> DeletePlan(long planId)
    {
        var result = await _learningPlanService.DeleteLearningPlanAsync(planId);
        if (!result)
            return NotFound(ApiResponse<object>.ErrorResponse("Plan not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Plan deleted"));
    }

    // ===== Daily Goal =====

    [HttpPost("daily-goal")]
    public async Task<IActionResult> SetDailyGoal([FromBody] SetDailyGoalRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var goal = await _learningPlanService.SetDailyGoalAsync(userId.Value, request.TargetQuestions, request.TargetMinutes);
        return Ok(ApiResponse<object>.SuccessResponse(goal, "Daily goal set"));
    }

    [HttpGet("daily-goal")]
    public async Task<IActionResult> GetDailyGoal()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var goal = await _learningPlanService.GetDailyGoalAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(goal));
    }

    [HttpGet("daily-goal/progress")]
    public async Task<IActionResult> GetDailyGoalProgress()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var progress = await _learningPlanService.GetDailyGoalProgressAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(progress));
    }

    // ===== Study Reminder =====

    [HttpPost("reminder")]
    public async Task<IActionResult> CreateReminder([FromBody] CreateReminderRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var reminderRequest = new StudyReminderRequest
        {
            Title = request.Title,
            ReminderTime = request.ReminderTime,
            DaysOfWeek = request.DaysOfWeek
        };

        var reminder = await _learningPlanService.CreateReminderAsync(userId.Value, reminderRequest);
        return Ok(ApiResponse<object>.SuccessResponse(reminder, "Reminder created"));
    }

    [HttpGet("reminders")]
    public async Task<IActionResult> GetReminders()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var reminders = await _learningPlanService.GetUserRemindersAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(reminders));
    }

    [HttpPut("reminder/{reminderId}/toggle")]
    public async Task<IActionResult> ToggleReminder(long reminderId, [FromBody] ToggleReminderRequest request)
    {
        var result = await _learningPlanService.ToggleReminderAsync(reminderId, request.IsEnabled);
        if (!result)
            return NotFound(ApiResponse<object>.ErrorResponse("Reminder not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Reminder updated"));
    }

    [HttpDelete("reminder/{reminderId}")]
    public async Task<IActionResult> DeleteReminder(long reminderId)
    {
        var result = await _learningPlanService.DeleteReminderAsync(reminderId);
        if (!result)
            return NotFound(ApiResponse<object>.ErrorResponse("Reminder not found"));

        return Ok(ApiResponse<object>.SuccessResponse(null, "Reminder deleted"));
    }

    // ===== Study Streak =====

    [HttpGet("streak")]
    public async Task<IActionResult> GetStreak()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var streak = await _learningPlanService.GetStudyStreakAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(streak));
    }

    // ===== Study Recommendation =====

    [HttpGet("recommendation")]
    public async Task<IActionResult> GetRecommendation()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var recommendation = await _learningPlanService.GetRecommendationAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(recommendation));
    }

    [HttpGet("today-plan")]
    public async Task<IActionResult> GetTodayPlan()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var plan = await _learningPlanService.GetTodayStudyPlanAsync(userId.Value);
        return Ok(ApiResponse<object>.SuccessResponse(plan));
    }
}

public class SetDailyGoalRequest
{
    public int TargetQuestions { get; set; } = 10;
    public int TargetMinutes { get; set; } = 30;
}

public class CreateReminderRequest
{
    public string Title { get; set; } = string.Empty;
    public System.TimeSpan ReminderTime { get; set; }
    public List<System.DayOfWeek> DaysOfWeek { get; set; } = new();
}

public class ToggleReminderRequest
{
    public bool IsEnabled { get; set; }
}

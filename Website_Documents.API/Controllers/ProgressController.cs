using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website_Documents.API.DTOs;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;

    public ProgressController(IProgressService progressService)
    {
        _progressService = progressService;
    }

    /// <summary>
    /// Get today's progress
    /// </summary>
    [HttpGet("today")]
    public async Task<IActionResult> GetTodayProgress()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var progress = await _progressService.GetTodayProgressAsync(userId.Value);
        
        if (progress == null)
        {
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                id = 0,
                userId = userId.Value,
                progressDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
                questionsAnswered = 0,
                questionsCorrect = 0,
                correctAnswers = 0,
                timeSpentMinutes = 0,
                studyMinutes = 0,
                examsCompleted = 0,
                xpEarned = 0
            }, "No progress data for today"));
        }

        var accuracyRate = progress.QuestionsAnswered > 0
            ? Math.Round((decimal)(progress.QuestionsCorrect ?? 0) / (progress.QuestionsAnswered ?? 1) * 100, 1)
            : 0m;

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            id = progress.Id,
            userId = progress.UserId,
            progressDate = progress.ProgressDate.ToString("yyyy-MM-dd"),
            questionsAnswered = progress.QuestionsAnswered ?? 0,
            questionsCorrect = progress.QuestionsCorrect ?? 0,
            correctAnswers = progress.QuestionsCorrect ?? 0,
            timeSpentMinutes = progress.TimeSpentMinutes ?? 0,
            studyMinutes = progress.TimeSpentMinutes ?? 0,
            examsCompleted = progress.ExamsCompleted ?? 0,
            xpEarned = progress.XpEarned ?? 0,
            accuracyRate = accuracyRate
        }, "Daily progress retrieved successfully"));
    }

    /// <summary>
    /// Get weekly progress
    /// </summary>
    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklyProgress()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var weeklyProgress = await _progressService.GetWeeklyProgressAsync(userId.Value);
        var dashboard = await _progressService.GetDashboardAsync(userId.Value);

        // Transform to frontend format
        var dailyXP = new int[7];
        var today = DateTime.UtcNow;
        var dayOfWeek = today.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)today.DayOfWeek - 1;
        
        for (int i = 0; i < 7; i++)
        {
            var targetDate = today.Date.AddDays(i - dayOfWeek);
            var dayProgress = weeklyProgress.FirstOrDefault(p => p.ProgressDate == DateOnly.FromDateTime(targetDate));
            dailyXP[i] = dayProgress != null ? (dayProgress.XpEarned ?? 0) : 0;
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            weekStart = today.Date.AddDays(-dayOfWeek).ToString("yyyy-MM-dd"),
            totalQuestions = dashboard.TotalQuestionsAnswered,
            correctQuestions = dashboard.TotalCorrectAnswers,
            totalMinutes = dashboard.TotalStudyTimeMinutes,
            totalExams = dashboard.TotalExamsTaken,
            totalXP = dailyXP.Sum(),
            averageScore = dashboard.AccuracyRate,
            bestDay = GetBestDay(weeklyProgress),
            streak = dashboard.CurrentStreak,
            dailyXP = dailyXP,
            dailyProgress = weeklyProgress.Select(p => new
            {
                date = p.ProgressDate.ToString("yyyy-MM-dd"),
                questionsAnswered = p.QuestionsAnswered ?? 0,
                correctAnswers = p.QuestionsCorrect ?? 0,
                timeSpentMinutes = p.TimeSpentMinutes ?? 0,
                xpEarned = p.XpEarned ?? 0
            }).ToList()
        }, "Weekly progress retrieved successfully"));
    }

    /// <summary>
    /// Get monthly stats
    /// </summary>
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyStats()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var stats = await _progressService.GetMonthlyStatsAsync(userId.Value);

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            year = stats.Year,
            month = stats.Month,
            monthName = stats.MonthName,
            totalQuestions = stats.TotalQuestions,
            correctQuestions = stats.CorrectQuestions,
            totalMinutes = stats.TotalMinutes,
            totalExams = stats.TotalExams,
            totalXP = stats.TotalXP,
            daysActive = stats.DaysActive,
            averageDailyXP = stats.AverageDailyXP
        }, "Monthly stats retrieved successfully"));
    }

    /// <summary>
    /// Get full dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var dashboard = await _progressService.GetDashboardAsync(userId.Value);

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            totalQuestionsAnswered = dashboard.TotalQuestionsAnswered,
            totalCorrectAnswers = dashboard.TotalCorrectAnswers,
            accuracyRate = dashboard.AccuracyRate,
            totalExamsTaken = dashboard.TotalExamsTaken,
            averageScore = dashboard.AverageScore,
            currentStreak = dashboard.CurrentStreak,
            longestStreak = dashboard.LongestStreak,
            totalStudyTimeMinutes = dashboard.TotalStudyTimeMinutes,
            lessonsCompleted = dashboard.LessonsCompleted,
            topicsCompleted = dashboard.TopicsCompleted,
            weeklyProgress = dashboard.WeeklyProgress.Select(p => new
            {
                date = p.Date.ToString("yyyy-MM-dd"),
                questionsAnswered = p.QuestionsAnswered,
                correctAnswers = p.CorrectAnswers,
                timeSpentMinutes = p.TimeSpentMinutes
            }).ToList()
        }, "Dashboard data retrieved successfully"));
    }

    /// <summary>
    /// Get topic progress
    /// </summary>
    [HttpGet("topics")]
    public async Task<IActionResult> GetTopicProgress()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var topicProgress = await _progressService.GetTopicProgressAsync(userId.Value);

        return Ok(ApiResponse<object>.SuccessResponse(
            topicProgress.Select(t => new
            {
                topicId = t.TopicId,
                topicName = t.TopicName,
                totalQuestions = t.TotalQuestions,
                correctCount = t.CorrectCount,
                accuracy = t.Accuracy,
                lastPracticed = t.LastPracticedAt?.ToString("yyyy-MM-ddTHH:mm:ssZ")
            }).ToList(), 
            "Topic progress retrieved successfully"
        ));
    }

    /// <summary>
    /// Get streak data
    /// </summary>
    [HttpGet("streak")]
    public async Task<IActionResult> GetStreak()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var streak = await _progressService.GetStreakAsync(userId.Value);

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            currentStreak = streak.CurrentStreak,
            longestStreak = streak.LongestStreak,
            lastActivityDate = streak.LastActivityDate?.ToString("yyyy-MM-ddTHH:mm:ssZ")
        }, "Streak data retrieved successfully"));
    }

    /// <summary>
    /// Update daily progress (force refresh)
    /// </summary>
    [HttpPost("update")]
    public async Task<IActionResult> UpdateProgress()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        var progress = await _progressService.UpdateDailyProgressAsync(userId.Value);
        var accuracyRate = progress.QuestionsAnswered > 0
            ? Math.Round((decimal)(progress.QuestionsCorrect ?? 0) / (progress.QuestionsAnswered ?? 1) * 100, 1)
            : 0m;

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            questionsAnswered = progress.QuestionsAnswered,
            correctAnswers = progress.QuestionsCorrect,
            timeSpentMinutes = progress.TimeSpentMinutes,
            xpEarned = progress.XpEarned,
            accuracyRate = accuracyRate
        }, "Progress updated successfully"));
    }

    /// <summary>
    /// Update lesson progress
    /// </summary>
    [HttpPost("lesson/{lessonId}")]
    public async Task<IActionResult> UpdateLessonProgress(int lessonId, [FromBody] UpdateProgressRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        await _progressService.UpdateLessonProgressAsync(userId.Value, lessonId, request.Status);
        
        // Get updated progress
        var dailyProgress = await _progressService.GetTodayProgressAsync(userId.Value);
        
        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            lessonId = lessonId,
            status = request.Status,
            dailyProgress = dailyProgress != null ? new
            {
                questionsAnswered = dailyProgress.QuestionsAnswered ?? 0,
                xpEarned = dailyProgress.XpEarned ?? 0
            } : null
        }, "Lesson progress updated successfully"));
    }

    /// <summary>
    /// Update topic progress
    /// </summary>
    [HttpPost("topic/{topicId}")]
    public async Task<IActionResult> UpdateTopicProgress(int topicId, [FromBody] UpdateProgressRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));

        await _progressService.UpdateTopicProgressAsync(userId.Value, topicId, request.Status);
        
        // Get updated progress
        var dailyProgress = await _progressService.GetTodayProgressAsync(userId.Value);
        
        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            topicId = topicId,
            status = request.Status,
            dailyProgress = dailyProgress != null ? new
            {
                questionsAnswered = dailyProgress.QuestionsAnswered ?? 0,
                xpEarned = dailyProgress.XpEarned ?? 0
            } : null
        }, "Topic progress updated successfully"));
    }

    private long? GetCurrentUserId()
    {
        // Try multiple claim types for user ID
        var userIdClaim = User.FindFirst("user_id") 
            ?? User.FindFirst("sub") 
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
            return userId;
        
        return null;
    }

    private string GetBestDay(List<Repository.Models.UserDailyProgress> weeklyProgress)
    {
        if (!weeklyProgress.Any()) return "T2";
        
        var best = weeklyProgress.OrderByDescending(p => p.XpEarned ?? 0).First();
        var dayIndex = best.ProgressDate.DayOfWeek == DayOfWeek.Sunday 
            ? 6 
            : (int)best.ProgressDate.DayOfWeek - 1;
        
        var days = new[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
        return days[dayIndex];
    }
}

public class UpdateProgressRequest
{
    public string Status { get; set; } = "in_progress";
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website_Documents.API.DTOs;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeminiController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly ILogger<GeminiController> _logger;

    public GeminiController(IGeminiService geminiService, ILogger<GeminiController> logger)
    {
        _geminiService = geminiService;
        _logger = logger;
    }

    /// <summary>
    /// Gửi prompt đơn giản đến Gemini AI
    /// </summary>
    [HttpPost("generate")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> GenerateContent([FromBody] GeminiPromptRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest(ApiResponse<string>.ErrorResponse("Prompt không được để trống"));
        }

        var response = await _geminiService.GenerateContentAsync(request.Prompt);

        if (!response.Success)
        {
            _logger.LogWarning("Gemini API failed: {Error}", response.ErrorMessage);
            return BadRequest(ApiResponse<string>.ErrorResponse(response.ErrorMessage ?? "Lỗi không xác định"));
        }

        return Ok(ApiResponse<string>.SuccessResponse(response.Text!));
    }

    /// <summary>
    /// Gửi tin nhắn với ngữ cảnh lịch sử (cho chatbot)
    /// </summary>
    [HttpPost("chat")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> Chat([FromBody] GeminiChatRequest request)
    {
        if (request.Messages == null || request.Messages.Count == 0)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse("Tin nhắn không được để trống"));
        }

        // Convert DTOs to service model
        var messages = request.Messages.Select(m => new GeminiMessage
        {
            Role = m.Role,
            Text = m.Text
        }).ToList();

        var response = await _geminiService.GenerateContentWithHistoryAsync(messages);

        if (!response.Success)
        {
            _logger.LogWarning("Gemini Chat API failed: {Error}", response.ErrorMessage);
            return BadRequest(ApiResponse<string>.ErrorResponse(response.ErrorMessage ?? "Lỗi không xác định"));
        }

        return Ok(ApiResponse<string>.SuccessResponse(response.Text!));
    }
}

public class GeminiPromptRequest
{
    public string Prompt { get; set; } = string.Empty;
}

public class GeminiChatRequest
{
    public List<GeminiMessageDto> Messages { get; set; } = new();
}

public class GeminiMessageDto
{
    public string Role { get; set; } = "user"; // "user" hoặc "model"
    public string Text { get; set; } = string.Empty;
}

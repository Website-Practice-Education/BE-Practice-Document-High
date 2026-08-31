namespace Website_Documents.Service.Interfaces;

public interface IGeminiService
{
    /// <summary>
    /// Gửi yêu cầu đến Gemini API và nhận phản hồi dạng text
    /// </summary>
    Task<GeminiResponse> GenerateContentAsync(string prompt);
    
    /// <summary>
    /// Gửi yêu cầu với danh sách tin nhắn để duy trì ngữ cảnh
    /// </summary>
    Task<GeminiResponse> GenerateContentWithHistoryAsync(List<GeminiMessage> messages);
}

public class GeminiMessage
{
    public string Role { get; set; } = "user"; // "user" hoặc "model"
    public string Text { get; set; } = string.Empty;
}

public class GeminiResponse
{
    public bool Success { get; set; }
    public string? Text { get; set; }
    public string? ErrorMessage { get; set; }
}

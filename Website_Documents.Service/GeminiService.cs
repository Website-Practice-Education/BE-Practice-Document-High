using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GeminiService> _logger;
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini API key is not configured");
        _logger = logger;
    }

    public async Task<GeminiResponse> GenerateContentAsync(string prompt)
    {
        try
        {
            var requestBody = new GeminiRequest
            {
                Contents = new List<ContentPart>
                {
                    new ContentPart
                    {
                        Parts = new List<Part>
                        {
                            new Part { Text = prompt }
                        }
                    }
                }
            };

            var url = $"{BaseUrl}/gemini-flash-latest:generateContent?key={_apiKey}";
            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return new GeminiResponse
                {
                    Success = false,
                    ErrorMessage = $"API returned status {(int)response.StatusCode}: {errorContent}"
                };
            }

            var responseContent = await response.Content.ReadFromJsonAsync<GeminiApiResponse>(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (responseContent?.Candidates == null || responseContent.Candidates.Count == 0)
            {
                return new GeminiResponse
                {
                    Success = false,
                    ErrorMessage = "No response from Gemini"
                };
            }

            var text = responseContent.Candidates[0].Content?.Parts?[0]?.Text;
            return new GeminiResponse
            {
                Success = true,
                Text = text ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
            return new GeminiResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<GeminiResponse> GenerateContentWithHistoryAsync(List<GeminiMessage> messages)
    {
        try
        {
            var requestBody = new GeminiRequest
            {
                Contents = messages.Select(m => new ContentPart
                {
                    Role = m.Role,
                    Parts = new List<Part>
                    {
                        new Part { Text = m.Text }
                    }
                }).ToList()
            };

            var url = $"{BaseUrl}/gemini-flash-latest:generateContent?key={_apiKey}";
            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return new GeminiResponse
                {
                    Success = false,
                    ErrorMessage = $"API returned status {(int)response.StatusCode}: {errorContent}"
                };
            }

            var responseContent = await response.Content.ReadFromJsonAsync<GeminiApiResponse>(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (responseContent?.Candidates == null || responseContent.Candidates.Count == 0)
            {
                return new GeminiResponse
                {
                    Success = false,
                    ErrorMessage = "No response from Gemini"
                };
            }

            var text = responseContent.Candidates[0].Content?.Parts?[0]?.Text;
            return new GeminiResponse
            {
                Success = true,
                Text = text ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API with history");
            return new GeminiResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}

// Request/Response DTOs for Gemini API
internal class GeminiRequest
{
    [JsonPropertyName("contents")]
    public List<ContentPart> Contents { get; set; } = new();
}

internal class ContentPart
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("parts")]
    public List<Part> Parts { get; set; } = new();
}

internal class Part
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

internal class GeminiApiResponse
{
    [JsonPropertyName("candidates")]
    public List<Candidate>? Candidates { get; set; }
}

internal class Candidate
{
    [JsonPropertyName("content")]
    public ContentPart? Content { get; set; }
}

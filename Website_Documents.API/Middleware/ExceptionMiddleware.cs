using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Website_Documents.API.DTOs;
using Microsoft.AspNetCore.Http;

namespace Website_Documents.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var apiResponse = exception switch
        {
            UnauthorizedAccessException => new { StatusCode = HttpStatusCode.Unauthorized, Message = exception.Message },
            InvalidOperationException => new { StatusCode = HttpStatusCode.BadRequest, Message = exception.Message },
            KeyNotFoundException => new { StatusCode = HttpStatusCode.NotFound, Message = exception.Message },
            _ => new { StatusCode = HttpStatusCode.InternalServerError, Message = "An unexpected error occurred" }
        };

        response.StatusCode = (int)apiResponse.StatusCode;

        var errorResponse = ApiResponse<object>.ErrorResponse(apiResponse.Message);
        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(json);
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}

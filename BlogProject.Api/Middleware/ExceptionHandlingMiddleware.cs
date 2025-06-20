using BlogProject.Core.CustomException;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlogProject.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Собираем контекстные данные для логов
        var requestId = context.TraceIdentifier;
        var referer = context.Request.Headers["Referer"].ToString();
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var user = context.User?.Identity?.Name ?? "Аноним";
        var method = context.Request.Method;
        var queryString = context.Request.QueryString;
        var clientIp = context.Connection.RemoteIpAddress?.ToString();

        try
        {
            await _next(context);
            if (context.Response.StatusCode is >= 400 and <= 599 && !context.Response.HasStarted)
            {
                await HandleStatusCodeAsync(context, requestId, referer, userAgent, user, clientIp);
            }
        }
        catch (Exception ex)
        {
            if (!context.Response.HasStarted)
            {
                await HandleExceptionAsync(context, ex, requestId, referer, userAgent, user, method, clientIp, queryString);
            }
            else
            {
                Log.Warning("Cannot handle exception: Response has already started for RequestID: {RequestId}", requestId);
            }

        }
    }

    private async Task HandleStatusCodeAsync(
        HttpContext context,
        string requestId,
        string referer,
        string userAgent,
        string user,
        string? clientIp)
    {
        var statusCode = context.Response.StatusCode;
        var message = GetStatusMessage(statusCode);

        Log.Warning(
            "ExceptionHandlingMiddleware: HTTP {StatusCode} - \nRequestID: {RequestId}, \nUser: {User}, \nIP: {ClientIp}, \nПуть: {Path}, \nИсточник: {Referer}, \nUserAgent: {UserAgent}",
            statusCode, requestId, user, clientIp, context.Request.Path, referer, userAgent
        );

        await WriteJsonErrorResponse(context, statusCode, message, requestId);
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        string requestId,
        string referer,
        string userAgent,
        string user,
        string method,
        string? clientIp,
        QueryString queryString)
    {
        var (statusCode, message) = GetExceptionDetails(exception);

        Log.Error("ExceptionHandlingMiddleware: Ошибка {StatusCode} - {Message}" +
                  "\nRequestID: {RequestId}, \nUser: {User}, \nМетод: {Method}, \nIP: {ClientIp}, \nПуть: {Path}, " +
                  "\nИсточник: {Referer}, \nUserAgent: {UserAgent},\nQueryString: {QueryString}",
             statusCode, message, requestId, user, method, clientIp, context.Request.Path, referer, userAgent, queryString
        );

        await WriteJsonErrorResponse(context, statusCode, message, requestId);
    }

    private (int statusCode, string message) GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            NotFoundException notFoundEx => (404, notFoundEx.Message),
            ForbiddenException forbiddenEx => (403, forbiddenEx.Message),
            DatabaseException dbEx => (503, dbEx.Message),
            ValidationException valEx => (400, valEx.Message),
            AppException appEx => (appEx.StatusCode, appEx.Message),
            _ => (500, _env.IsDevelopment() ? exception.Message : "Произошла ошибка.")
        };
    }

    private string GetStatusMessage(int statusCode)
    {
        return statusCode switch
        {
            404 => "Ресурс не найден",
            401 => "Неавторизованный доступ",
            403 => "Доступ запрещён",
            405 => "Метод не поддерживается",
            500 => "Внутренняя ошибка сервера",
            _ => $"Ошибка {statusCode}"
        };
    }

    private async Task WriteJsonErrorResponse(HttpContext context, int statusCode, string message, string requestId)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            StatusCode = statusCode,
            Message = message,
            RequestId = requestId,
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
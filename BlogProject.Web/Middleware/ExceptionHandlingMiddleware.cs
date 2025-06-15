using BlogProject.Core.CustomException;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using System.Net;

namespace BlogProject.Web.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Собираем контекстные данные для логов
        var requestId = context.TraceIdentifier;
        var referer = context.Request.Headers["Referer"].ToString();
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var user = context.User?.Identity?.Name ?? "Аноним";
        var method = context.Request.Method;
        var path = context.Request.Path;
        var queryString = context.Request.QueryString;

        try
        {
            await next(context);
            if (context.Response.StatusCode is >= 400 and <= 599)
            {
                await HandleStatusCodeAsync(context, requestId, referer, userAgent, user);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex,
                "ExceptionHandlingMiddleware: Необработанное исключение. " +
               "\nRequestID: {RequestId}, \nUser: {User}, \nМетод: {Method}, \nПуть: {Path}, " +
                "\nИсточник: {Referer}, \nUserAgent: {UserAgent}, \nQueryString: {QueryString}",
                requestId, user, method, path, referer, userAgent, queryString
            );

            await HandleExceptionAsync(context, ex, requestId, referer, userAgent, user);
        }
    }

    private static async Task HandleStatusCodeAsync(
        HttpContext context,
        string requestId,
        string referer,
        string userAgent,
        string user)
    {
        var statusCode = context.Response.StatusCode;
        var feature = context.Features.Get<IStatusCodeReExecuteFeature>();
        var originalPath = feature?.OriginalPath ?? context.Request.Path;

        Log.Warning(
            "ExceptionHandlingMiddleware: HTTP {StatusCode} - \nRequestID: {RequestId}, \nUser: {User}, \nПуть: {Path}, \nИсточник: {Referer}, \nUserAgent: {UserAgent}",
            statusCode, requestId, user, originalPath, referer, userAgent
        );

        if (statusCode is >= 400 and <= 599)
        {
            string message = statusCode switch
            {
                404 => "Страница не найдена",
                401 => "Неавторизованный доступ",
                403 => "Доступ запрещён",
                405 => "Метод не поддерживается",
                500 => context.Features.Get<IExceptionHandlerFeature>()?.Error?.Message ?? "Внутренняя ошибка сервера",
                _ => $"Ошибка {statusCode}"
            };

            var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (statusCode == 500 && exceptionFeature?.Error != null)
            {
                Log.Error(exceptionFeature.Error,
                    "Ошибка обработки запроса - " +
                    "RequestID: {RequestId}, Путь: {Path}, User: {User}",
                    requestId, originalPath, user);
            }

            context.Response.Redirect($"/Error/Index?statusCode={statusCode}&requestId={requestId}&message={WebUtility.UrlEncode(message)}");
            await Task.CompletedTask;
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        string requestId,
        string referer,
        string userAgent,
        string user)
    {
        var (statusCode, message) = exception switch
        {
            NotFoundException notFoundEx => (404, notFoundEx.Message),
            ForbiddenException forbiddenEx => (403, forbiddenEx.Message),
            DatabaseException dbEx => (503, dbEx.Message),
            ValidationException valEx => (400, valEx.Message),
            AppException appEx => (appEx.StatusCode, appEx.Message),
            _ => (500, env.IsDevelopment() ? exception.Message : "Произошла ошибка.")
        };

        Log.Error(exception,
            "HandleExceptionAsync: Ошибка {StatusCode} - " +
            "\nRequestID: {RequestId}, \nUser: {User}, \nИсточник: {Referer}, \nUserAgent: {UserAgent}",
            statusCode, requestId, user, referer, userAgent);

        context.Response.StatusCode = statusCode;
        context.Response.Redirect($"/Error/Index?statusCode={statusCode}&requestId={requestId}&message={WebUtility.UrlEncode(message)}");
        await Task.CompletedTask;
    }
}
using BlogProject.Core.CustomException;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using System.Net;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace BlogProject.Web.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
            // Обработка статусных кодов после выполнения pipeline
            if (context.Response.StatusCode is >= 400 and <= 599)
            {
                await HandleStatusCodeAsync(context);
            }
        }
        catch (Exception ex)
        {
            await Task.Run(() => Log.Error(ex, "Unhandled exception occurred"));
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleStatusCodeAsync(HttpContext context)
    {
        var statusCode = context.Response.StatusCode;
        var feature = context.Features.Get<IStatusCodeReExecuteFeature>();
        var originalPath = feature?.OriginalPath ?? context.Request.Path;

        await Task.Run(() => Log.Warning("HTTP {StatusCode} for path: {Path}", statusCode, originalPath));

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
                logger.LogError(exceptionFeature.Error, "Error processing request for path: {Path}", originalPath);
            }

            context.Response.Redirect($"/Error/Index?statusCode={statusCode}&message={WebUtility.UrlEncode(message)}");
            await Task.CompletedTask;
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            NotFoundException notFoundEx => (404, notFoundEx.Message),
            ForbiddenException forbiddenEx => (403, forbiddenEx.Message),
            DatabaseException dbEx => (503, dbEx.Message),
            ValidationException valEx => (400, valEx.Message),
            AppException appEx => (appEx.StatusCode, appEx.Message),
            _ => (500, env.IsDevelopment() ? exception.Message : "An error occurred")
        };

        context.Response.StatusCode = statusCode;
        await Task.Run(() =>
        {
            context.Response.Redirect($"/Error/Index?statusCode={statusCode}&message={WebUtility.UrlEncode(message)}");
        });
    }
}
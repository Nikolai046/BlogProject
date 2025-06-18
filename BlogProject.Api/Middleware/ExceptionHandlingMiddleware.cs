using BlogProject.Core.CustomException;
using System.Text.Json;
using Serilog;

namespace BlogProject.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Передаём управление следующему middleware
            await next(context);
        }
        catch (Exception ex)
        {
            // Обрабатываем исключение
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Определяем код ответа и сообщение
        var (statusCode, errorMessage) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Ресурс не найден"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Несанкционированный доступ"),
            System.InvalidOperationException => (StatusCodes.Status400BadRequest, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Произошла непредвиденная ошибка.")
        };

        // Логируем ошибку с уровнем Error
        Log.Error(exception, "Произошла ошибка при обработке запроса: {Message}", exception.Message);

        // Формируем JSON-ответ
        var response = new
        {
            StatusCode = statusCode,
            Error = errorMessage,
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };

        // Устанавливаем заголовки ответа
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        // Сериализуем ответ в JSON
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}

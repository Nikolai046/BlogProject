namespace BlogProject.Web.Middleware;

public static class CustomExceptionHandlingExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
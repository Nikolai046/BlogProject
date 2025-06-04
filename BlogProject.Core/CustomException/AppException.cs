namespace BlogProject.Core.CustomException;

public class AppException(string message, int statusCode = 400, string redirectPath = "/Error")
    : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string RedirectPath { get; } = redirectPath;
}
namespace BlogProject.Core.CustomException;

public class AppException : Exception
{
    public int StatusCode { get; }
    public string RedirectPath { get; }

    public AppException(string message, int statusCode = 400, string redirectPath = "/Error") : base(message)
    {
        StatusCode = statusCode;
        RedirectPath = redirectPath;
    }
}
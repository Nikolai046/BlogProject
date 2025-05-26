namespace BlogProject.Core.CustomException;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message, 403)
    {

    }
}
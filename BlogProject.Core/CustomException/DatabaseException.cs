namespace BlogProject.Core.CustomException;

public class DatabaseException : AppException
{
    public DatabaseException(string message)
        : base(message, 503, "/Error/Database")
    {
    }
}
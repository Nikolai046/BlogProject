namespace BlogProject.Core.CustomException;

public class DatabaseException(string message) : AppException(message, 503, "/Error/Database");
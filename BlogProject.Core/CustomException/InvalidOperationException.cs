namespace BlogProject.Core.CustomException;

public class InvalidOperationException(string field) : AppException($"{field}");

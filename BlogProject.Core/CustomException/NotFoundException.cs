namespace BlogProject.Core.CustomException;

public class NotFoundException(string entityName) : AppException(message: $"\"{entityName}\" не найден(о)", statusCode: 404,
    redirectPath: "/Error/NotFound");
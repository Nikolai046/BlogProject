namespace BlogProject.Core.CustomException;

public class ValidationException(string field) : AppException(message: $"Ошибка валидации поля \"{field}\"",
    statusCode: 400, redirectPath: "/Error/BadRequest");
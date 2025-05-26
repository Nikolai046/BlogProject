namespace BlogProject.Core.CustomException;

public class ValidationException : AppException
{
    public ValidationException(string field)
        : base(message: $"Ошибка валидации поля {field}", statusCode: 400, redirectPath: "/Error/BadRequest")
    {
    }
}
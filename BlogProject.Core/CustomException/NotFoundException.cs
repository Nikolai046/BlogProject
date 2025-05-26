namespace BlogProject.Core.CustomException;

public class NotFoundException : AppException
{
    public NotFoundException(string entityName)
        : base(message: $"{entityName} не найдено", statusCode: 404, redirectPath: "/Error/NotFound")
    {
    }
}
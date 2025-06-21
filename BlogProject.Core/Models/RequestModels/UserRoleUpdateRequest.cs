using System.ComponentModel.DataAnnotations;

namespace BlogProject.Core.Models.RequestModels;

public class UserRoleUpdateRequest
{
    [Required(ErrorMessage = "Заполните это поле")]
    public string UserId { get; set; }

    [Required(ErrorMessage = "Заполните это поле")]
    [RegularExpression("^(Administrator|Moderator|User)$", ErrorMessage = "Недопустимая роль пользователя")]
    public string UserRole { get; set; }
}
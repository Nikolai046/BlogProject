using System.ComponentModel.DataAnnotations;

namespace BlogProject.Core.Models.RequestModels;

public class UserEditRequest
{
    [Required(ErrorMessage = "Заполните это поле")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Заполните это поле")]
    public string? LastName { get; set; }

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Введите текущий пароль")]
    public string? CurrentPassword { get; set; }

    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "Поле {0} должно иметь минимум {2} и максимум {1} символов.", MinimumLength = 5)]
    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
    public string? ConfirmPassword { get; set; }
}
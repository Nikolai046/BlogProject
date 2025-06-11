using System.ComponentModel.DataAnnotations;

namespace BlogProject.Core.Models.ViewModels;

public class UpdateUserViewModel
{
    [Required(ErrorMessage = "Заполните это поле")]
    [Display(Name = "Имя")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Заполните это поле")]
    [Display(Name = "Фамилия")]
    public string? LastName { get; set; }

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Введите текущий пароль")]
    [Display(Name = "Текущий пароль")]
    public string CurrentPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "Поле {0} должно иметь минимум {2} и максимум {1} символов.", MinimumLength = 5)]
    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Подтверждение пароля")]
    public string? ConfirmPassword { get; set; }
}
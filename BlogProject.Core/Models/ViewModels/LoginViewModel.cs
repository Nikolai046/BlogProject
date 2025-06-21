using System.ComponentModel.DataAnnotations;

namespace BlogProject.Core.Models.ViewModels;

public class LoginViewModel
{
    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Пожалуйста, введите корректный email.")]
    [Display(Name = "e-mail")]
    public string Email { get; init; }

    [StringLength(100, ErrorMessage = "Поле {0} должно иметь минимум {2} и максимум {1} символов.", MinimumLength = 5)]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Заполните это поле")]
    [Display(Name = "Пароль")]
    public string? Password { get; init; }

    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; init; }
}
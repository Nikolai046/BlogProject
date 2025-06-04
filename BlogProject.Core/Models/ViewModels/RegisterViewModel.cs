using System.ComponentModel.DataAnnotations;

namespace BlogProject.Core.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Заполните это поле")]
    [Display(Name = "Имя")]
    public required string? FirstName { get; set; }

    [Required(ErrorMessage = "Заполните это поле")]
    [Display(Name = "Фамилия")]
    public required string? LastName { get; set; }

    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Пожалуйста, введите корректный email.")]
    [Display(Name = "e-mail")]
    public required string? Email { get; set; }

    [StringLength(100, ErrorMessage = "Поле {0} должно иметь минимум {2} и максимум {1} символов.", MinimumLength = 5)]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Заполните это поле")]
    [Display(Name = "Новый пароль")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Подтверждение пароля")]
    public string? ConfirmPassword { get; set; }
}
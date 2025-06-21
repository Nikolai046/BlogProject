using System.ComponentModel.DataAnnotations;

namespace BlogProject.Core.Models.RequestModels;

public class RegisterRequest
{
    [Required(ErrorMessage = "Заполните это поле")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Заполните это поле")]
    public string LastName { get; set; }

    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Пожалуйста, введите корректный email.")]
    public string Email { get; set; }

    [StringLength(100, ErrorMessage = "Поле {0} должно иметь минимум {2} и максимум {1} символов.", MinimumLength = 5)]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Заполните это поле")]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
    [Required]
    public string ConfirmPassword { get; set; }
}
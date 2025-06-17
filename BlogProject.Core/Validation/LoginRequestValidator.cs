using BlogProject.Core.Models.RequestModels;
using FluentValidation;

namespace BlogProject.Core.Validation;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Введите корректный email")
            .NotEmpty().WithMessage("Введите email");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Введите пароль");
    }
}

using Health.Api.Models.Requests;
using FluentValidation;

namespace Health.Api.Validators;

public class AdminLoginRequestValidator : AbstractValidator<AdminLoginRequest>
{
    public AdminLoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty();
        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
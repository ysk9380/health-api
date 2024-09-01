using Health.Api.Models.Requests;
using FluentValidation;

namespace Health.Api.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.CustomerCode)
            .NotEmpty();
        RuleFor(x => x.Username)
            .NotEmpty();
        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
using FluentValidation;
using Health.Api.Models.Requests;

namespace Health.Api.Validators;

public class CreateLoginRequestValidator : AbstractValidator<CreateLoginRequest>
{
    public CreateLoginRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(default(long));
        RuleFor(x => x.AppUserId)
            .GreaterThan(default(long));
        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(50);
        RuleFor(x => x.LanguageCode)
            .NotEmpty();
        RuleFor(x => x.RoleCode)
            .NotEmpty();
    }
}
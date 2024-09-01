using FluentValidation;
using Health.Api.Models.Requests.User;

namespace Health.Api.Validators;

public class UpdateAppUserEmailRequestValidator : AbstractValidator<UpdateAppUserEmailRequest>
{
    public UpdateAppUserEmailRequestValidator()
    {
        RuleFor(x => x.AppUserEmailId)
            .GreaterThan(default(long));
        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);
    }
}
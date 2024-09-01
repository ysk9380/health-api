using FluentValidation;
using Health.Api.Models.Requests.User;

namespace Health.Api.Validators.User;

public class InsertAppUserEmailRequestValidator : AbstractValidator<InsertAppUserEmailRequest>
{
    public InsertAppUserEmailRequestValidator()
    {
        RuleFor(x => x.AppUserId)
            .GreaterThan(default(long));
        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);
    }
}
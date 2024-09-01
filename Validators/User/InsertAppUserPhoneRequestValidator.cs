using FluentValidation;
using Health.Api.Models.Requests.User;

namespace Health.Api.Validators.User;

public class InsertAppUserPhoneRequestValidator : AbstractValidator<InsertAppUserPhoneRequest>
{
    public InsertAppUserPhoneRequestValidator()
    {
        RuleFor(x => x.AppUserId)
            .GreaterThan(default(long));
        RuleFor(x => x.PhoneTypeCode)
            .NotEmpty();
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(15);
        RuleFor(x => x.ListedAs)
            .MaximumLength(100).When(c => !string.IsNullOrEmpty(c.ListedAs));
    }
}
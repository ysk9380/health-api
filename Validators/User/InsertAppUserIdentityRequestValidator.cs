using FluentValidation;
using Health.Api.Models.Requests.User;

namespace Health.Api.Validators.Patient;

public class InsertAppUserIdentityRequestValidator : AbstractValidator<InsertAppUserIdentityRequest>
{
    public InsertAppUserIdentityRequestValidator()
    {
        RuleFor(x => x.AppUserId)
            .GreaterThan(default(long));
        RuleFor(x => x.IdentityTypeCode)
            .NotEmpty();
        RuleFor(x => x.IdentityNumber)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.IssuedBy)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.PlaceIssued)
            .NotEmpty()
            .MaximumLength(100);
    }
}
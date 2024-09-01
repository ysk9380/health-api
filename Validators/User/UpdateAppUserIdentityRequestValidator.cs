using FluentValidation;
using Health.Api.Models.Requests.User;

namespace Health.Api.Validators.Patient;

public class UpdateAppUserIdentityRequestValidator : AbstractValidator<UpdateAppUserIdentityRequest>
{
    public UpdateAppUserIdentityRequestValidator()
    {
        RuleFor(x => x.AppUserIdentityId)
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
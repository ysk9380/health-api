using FluentValidation;
using Health.Api.Models.Requests.User;

namespace Health.Api.Validators.User;

public class InsertAppUserAddressRequestValidator : AbstractValidator<InsertAppUserAddressRequest>
{
    public InsertAppUserAddressRequestValidator()
    {
        RuleFor(x => x.AppUserId)
            .GreaterThan(default(long));
        RuleFor(x => x.AddressTypeCode)
            .NotEmpty();
        RuleFor(x => x.AddressLine1)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.AddressLine2)
            .NotEmpty()
            .MaximumLength(100).When(c => !string.IsNullOrEmpty(c.AddressLine2));
        RuleFor(x => x.AddressLine2)
            .NotEmpty()
            .MaximumLength(100).When(c => !string.IsNullOrEmpty(c.AddressLine3));
        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.Pincode)
            .NotEmpty()
            .MaximumLength(15);
        RuleFor(x => x.Pincode)
            .NotEmpty();
        RuleFor(x => x.StateCode)
            .NotEmpty();
    }
}
using FluentValidation;
using Health.Api.Models.Requests.User;

namespace Health.Api.Validators.User;

public class UpdateAppUserhoneRequestValidator : AbstractValidator<UpdateAppUserPhoneRequest>
{
    public UpdateAppUserhoneRequestValidator()
    {
        RuleFor(x => x.AppUserPhoneId)
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
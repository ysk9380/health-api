using Health.Api.Models.Requests;
using FluentValidation;

namespace Health.Api.Validators;

public class ContactRequestValidator : AbstractValidator<ContactRequest>
{
    public ContactRequestValidator()
    {
        RuleFor(x => x.MarketId).NotEqual(0);
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);
        RuleFor(x => x.Mobile)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.Address)
            .MaximumLength(250).When(c => !string.IsNullOrEmpty(c.Address));
    }
}

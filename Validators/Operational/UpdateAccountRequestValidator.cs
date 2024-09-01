using FluentValidation;
using Health.Api.Models.Requests.Operational;

namespace Health.Api.Validators.User;

public class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        RuleFor(x => x.AccountId)
            .GreaterThan(default(byte));

        RuleFor(x => x.AccountCode)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.AccountName)
            .NotEmpty()
            .MaximumLength(250);
    }
}
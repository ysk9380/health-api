using FluentValidation;
using Health.Api.Models.Requests.Operational;

namespace Health.Api.Validators.User;

public class InsertAccountRequestValidator : AbstractValidator<InsertAccountRequest>
{
    public InsertAccountRequestValidator()
    {
        RuleFor(x => x.AccountCode)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.AccountName)
            .NotEmpty()
            .MaximumLength(250);
    }
}
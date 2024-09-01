using FluentValidation;
using Health.Api.Models.Requests.User;

namespace Health.Api.Validators.User;

public class InsertAppUserRequestValidator : AbstractValidator<InsertAppUserRequest>
{
    public InsertAppUserRequestValidator()
    {
        RuleFor(x => x.Firstname)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.Lastname)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.DateOfBirth.Date)
            .LessThanOrEqualTo(DateTime.Today);
        RuleFor(x => x.GenderCode)
            .NotEmpty();
    }
}
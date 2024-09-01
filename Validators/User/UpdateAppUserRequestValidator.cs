using FluentValidation;
using Health.Api.Models.Requests.User;

namespace Health.Api.Validators.User;

public class UpdateAppUserRequestValidator : AbstractValidator<UpdateAppUserRequest>
{
    public UpdateAppUserRequestValidator()
    {
        RuleFor(x => x.AppUserId)
            .NotEmpty();
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
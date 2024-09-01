using FluentValidation;
using Health.Api.Models.Requests.User;

namespace Health.Api.Validators.User;

public class ChangeAppUserPasswordRequestValidator : AbstractValidator<ChangeAppUserPasswordRequest>
{
    public ChangeAppUserPasswordRequestValidator()
    {
        RuleFor(x => x.CustomerAppUserId)
            .NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty();
    }
}
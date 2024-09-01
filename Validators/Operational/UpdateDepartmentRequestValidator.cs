using FluentValidation;
using Health.Api.Models.Requests.Operational;

namespace Health.Api.Validators.User;

public class UpdateDepartmentRequestValidator : AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentRequestValidator()
    {
        RuleFor(x => x.DepartmentId)
            .GreaterThan(default(byte));

        RuleFor(x => x.DepartmentCode)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DepartmentName)
            .NotEmpty()
            .MaximumLength(250);
    }
}
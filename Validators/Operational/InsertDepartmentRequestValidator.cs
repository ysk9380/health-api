using FluentValidation;
using Health.Api.Models.Requests.Operational;

namespace Health.Api.Validators.User;

public class InsertDepartmentRequestValidator : AbstractValidator<InsertDepartmentRequest>
{
    public InsertDepartmentRequestValidator()
    {
        RuleFor(x => x.DepartmentCode)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.DepartmentName)
            .NotEmpty()
            .MaximumLength(250);
    }
}
using FluentValidation;
using Health.Api.Models.Requests.Operational;

namespace Health.Api.Validators.User;

public class InsertServiceRequestValidator : AbstractValidator<InsertServiceRequest>
{
    public InsertServiceRequestValidator()
    {
        RuleFor(x => x.ServiceCode)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .MaximumLength(250);
    }
}
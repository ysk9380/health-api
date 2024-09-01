using FluentValidation;
using Health.Api.Models.Requests.Operational;

namespace Health.Api.Validators.User;

public class UpdateServiceRequestValidator : AbstractValidator<UpdateServiceRequest>
{
    public UpdateServiceRequestValidator()
    {
        RuleFor(x => x.ServiceId)
            .GreaterThan(default(byte));

        RuleFor(x => x.ServiceCode)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .MaximumLength(250);
    }
}
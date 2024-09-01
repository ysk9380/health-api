using FluentValidation;
using Health.Api.Models.Requests.Patient;

namespace Health.Api.Validators;

public class UpdatePatientEmailRequestValidator : AbstractValidator<UpdatePatientEmailRequest>
{
    public UpdatePatientEmailRequestValidator()
    {
        RuleFor(x => x.PatientEmailId)
            .GreaterThan(default(long));
        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);
    }
}
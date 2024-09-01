using FluentValidation;
using Health.Api.Models.Requests.Patient;

namespace Health.Api.Validators.Patient;

public class InsertPatientEmailRequestValidator : AbstractValidator<InsertPatientEmailRequest>
{
    public InsertPatientEmailRequestValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(default(long));
        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);
    }
}
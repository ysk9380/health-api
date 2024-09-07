using FluentValidation;
using Health.Api.Models.Requests.Patient;

namespace Health.Api.Validators.Patient;

public class InsertPatientHealthVitalRequestValidator : AbstractValidator<InsertPatientHealthVitalRequest>
{
    public InsertPatientHealthVitalRequestValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(default(long));
        RuleFor(x => x.HealthVitalCode)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.Content)
            .NotEmpty();
    }
}
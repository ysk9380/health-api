using FluentValidation;
using Health.Api.Models.Requests.Patient;

namespace Health.Api.Validators.Patient;

public class InsertPatientPhoneRequestValidator : AbstractValidator<InsertPatientPhoneRequest>
{
    public InsertPatientPhoneRequestValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(default(long));
        RuleFor(x => x.PhoneTypeCode)
            .NotEmpty();
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(15);
        RuleFor(x => x.ListedAs)
            .MaximumLength(100).When(c => !string.IsNullOrEmpty(c.ListedAs));
    }
}
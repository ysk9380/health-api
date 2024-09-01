using FluentValidation;
using Health.Api.Models.Requests.Patient;

namespace Health.Api.Validators.Patient;

public class InsertPatientIdentityRequestValidator : AbstractValidator<InsertPatientIdentityRequest>
{
    public InsertPatientIdentityRequestValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(default(long));
        RuleFor(x => x.IdentityTypeCode)
            .NotEmpty();
        RuleFor(x => x.IdentityNumber)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.IssuedBy)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.PlaceIssued)
            .NotEmpty()
            .MaximumLength(100);
    }
}
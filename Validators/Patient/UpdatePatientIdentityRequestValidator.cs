using FluentValidation;
using Health.Api.Models.Requests.Patient;

namespace Health.Api.Validators.Patient;

public class UpdatePatientIdentityRequestValidator : AbstractValidator<UpdatePatientIdentityRequest>
{
    public UpdatePatientIdentityRequestValidator()
    {
        RuleFor(x => x.PatientIdentityId)
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
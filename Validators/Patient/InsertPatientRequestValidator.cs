using FluentValidation;
using Health.Api.Models.Requests.Patient;

namespace Health.Api.Validators.Patient;

public class InsertPatientRequestValidator : AbstractValidator<InsertPatientRequest>
{
    public InsertPatientRequestValidator()
    {
        RuleFor(x => x.Firstname)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.Middlename)
            .MaximumLength(100).When(p => !string.IsNullOrEmpty(p.Middlename));
        RuleFor(x => x.Lastname)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.DateOfBirth.Date)
            .LessThanOrEqualTo(DateTime.Today);
        RuleFor(x => x.GenderCode)
            .NotEmpty();
    }
}
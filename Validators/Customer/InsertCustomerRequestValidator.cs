using FluentValidation;
using Health.Api.Models.Requests.Customer;

namespace Health.Api.Validators.Customer;

public class InsertCustomerRequestValidator : AbstractValidator<InsertCustomerRequest>
{
    public InsertCustomerRequestValidator()
    {
        RuleFor(x => x.CustomerShortName)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .MaximumLength(250);
    }
}
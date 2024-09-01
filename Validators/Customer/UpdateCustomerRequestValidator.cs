using FluentValidation;
using Health.Api.Models.Requests.Customer;

namespace Health.Api.Validators.Customer;

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(default(long));
        RuleFor(x => x.CustomerShortName)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .MaximumLength(250);
    }
}
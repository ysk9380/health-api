namespace Health.Api.Models.Requests.Customer;

public class UpdateCustomerRequest
{
    public long CustomerId { get; set; } = default!;
    public string CustomerShortName { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
}
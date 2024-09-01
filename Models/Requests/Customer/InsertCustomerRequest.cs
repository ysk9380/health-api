namespace Health.Api.Models.Requests.Customer;

public class InsertCustomerRequest
{
    public string CustomerShortName { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
}
using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("Customer")]
public class Customer
{
    public long CustomerId { get; set; }
    public string CustomerCode { get; set; } = default!;
    public string CustomerShortName { get; set; } = default!;
    public string CustomerName { get; set; } = default!;

    public Customer(Customer copy)
    {
        CustomerId = copy.CustomerId;
        CustomerCode = copy.CustomerCode;
        CustomerShortName = copy.CustomerShortName;
        CustomerName = copy.CustomerName;
    }

    public Customer()
    { }
}
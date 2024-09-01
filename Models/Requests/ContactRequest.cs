namespace Health.Api.Models.Requests;

public class ContactRequest
{
    public int MarketId { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Mobile { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string Note { get; set; } = default!;
    public bool ArrangeDemo { get; set; }
}
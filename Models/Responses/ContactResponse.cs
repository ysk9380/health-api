namespace Health.Api.Models.Responses;

public class ContactResponse
{
    public int MarketId { get; set; }
    public string MarketName { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string MobileNumber { get; set; } = default!;
    public string Address { get; set; } = default!;
}
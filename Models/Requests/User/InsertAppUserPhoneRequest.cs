namespace Health.Api.Models.Requests.User;

public class InsertAppUserPhoneRequest
{
    public long AppUserId { get; set; }
    public string PhoneTypeCode { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string ListedAs { get; set; } = default!;
}
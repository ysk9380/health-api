namespace Health.Api.Models.Requests.User;

public class UpdateAppUserPhoneRequest
{
    public long AppUserPhoneId { get; set; }
    public string PhoneTypeCode { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string ListedAs { get; set; } = default!;
}
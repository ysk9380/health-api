namespace Health.Api.Models.User;

public class AppUserPhoneModel
{
    public long AppUserPhoneId { get; set; }
    public long AppUserId { get; set; }
    public string PhoneNumber { get; set; } = default!;
    public string? ListedAs { get; set; }
    public string PhoneTypeCode { get; set; } = default!;
    public string PhoneTypeName { get; set; } = default!;
}
namespace Health.Api.Models.User;

public class AppUserIdentityModel
{
    public long AppUserIdentityId { get; set; }
    public long AppUserId { get; set; }
    public string IdentityNumber { get; set; } = default!;
    public string IssuedBy { get; set; } = default!;
    public string? PlaceIssued { get; set; }
    public DateTime? Expiry { get; set; }
    public string IdentityTypeCode { get; set; } = default!;
    public string IdentityTypeName { get; set; } = default!;
}
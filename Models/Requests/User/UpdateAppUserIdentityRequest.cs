namespace Health.Api.Models.Requests.User;

public class UpdateAppUserIdentityRequest
{
    public long AppUserIdentityId { get; set; }
    public string IdentityTypeCode { get; set; } = default!;
    public string IdentityNumber { get; set; } = default!;
    public string IssuedBy { get; set; } = default!;
    public string? PlaceIssued { get; set; }
    public DateTime? Expiry { get; set; }
}
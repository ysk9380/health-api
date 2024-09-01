using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("AppUserIdentity")]
public class AppUserIdentity
{
    public long AppUserIdentityId { get; set; }
    public long AppUserId { get; set; }
    public byte IdentityTypeId { get; set; }
    public string IdentityNumber { get; set; } = default!;
    public string IssuedBy { get; set; } = default!;
    public string? PlaceIssued { get; set; }
    public DateTime? Expiry { get; set; }
    public bool IsActive { get; set; }

    public AppUserIdentity(AppUserIdentity identity)
    {
        AppUserIdentityId = identity.AppUserIdentityId;
        AppUserId = identity.AppUserId;
        IdentityTypeId = identity.IdentityTypeId;
        IdentityNumber = identity.IdentityNumber;
        IssuedBy = identity.IssuedBy;
        PlaceIssued = identity.PlaceIssued;
        Expiry = identity.Expiry;
        IsActive = identity.IsActive;
    }

    public AppUserIdentity() { }
}
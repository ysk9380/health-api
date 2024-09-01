using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("PatientIdentity")]
public class PatientIdentity
{
    public long PatientIdentityId { get; set; }
    public long PatientId { get; set; }
    public byte IdentityTypeId { get; set; }
    public string IdentityNumber { get; set; } = default!;
    public string IssuedBy { get; set; } = default!;
    public string? PlaceIssued { get; set; }
    public DateTime? Expiry { get; set; }
    public bool IsActive { get; set; }

    public PatientIdentity(PatientIdentity identity)
    {
        PatientIdentityId = identity.PatientIdentityId;
        PatientId = identity.PatientId;
        IdentityTypeId = identity.IdentityTypeId;
        IdentityNumber = identity.IdentityNumber;
        IssuedBy = identity.IssuedBy;
        PlaceIssued = identity.PlaceIssued;
        Expiry = identity.Expiry;
        IsActive = identity.IsActive;
    }

    public PatientIdentity() { }
}
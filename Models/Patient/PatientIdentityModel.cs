namespace Health.Api.Models.Patient;

public class PatientIdentityModel
{
    public long PatientIdentityId { get; set; }
    public long PatientId { get; set; }
    public string IdentityNumber { get; set; } = default!;
    public string IssuedBy { get; set; } = default!;
    public string? PlaceIssued { get; set; }
    public DateTime? Expiry { get; set; }
    public string IdentityTypeCode { get; set; } = default!;
    public string IdentityTypeName { get; set; } = default!;
}
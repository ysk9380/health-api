namespace Health.Api.Models.Requests.Patient;

public class UpdatePatientIdentityRequest
{
    public long PatientIdentityId { get; set; }
    public string IdentityTypeCode { get; set; } = default!;
    public string IdentityNumber { get; set; } = default!;
    public string IssuedBy { get; set; } = default!;
    public string? PlaceIssued { get; set; }
    public DateTime? Expiry { get; set; }
}
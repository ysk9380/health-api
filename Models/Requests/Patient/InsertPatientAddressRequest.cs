namespace Health.Api.Models.Requests.Patient;

public class InsertPatientAddressRequest
{
    public long PatientId { get; set; }
    public string AddressTypeCode { get; set; } = default!;
    public string AddressLine1 { get; set; } = default!;
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string City { get; set; } = default!;
    public string Pincode { get; set; } = default!;
    public string StateCode { get; set; } = default!;
}
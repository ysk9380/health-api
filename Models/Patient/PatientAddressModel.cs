namespace Health.Api.Models.Patient;

public class PatientAddressModel
{
    public long PatientAddressId { get; set; }
    public long PatientId { get; set; }
    public string AddressLine1 { get; set; } = default!;
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string City { get; set; } = default!;
    public string Pincode { get; set; } = default!;
    public string StateCode { get; set; } = default!;
    public string StateName { get; set; } = default!;
    public string AddressTypeCode { get; set; } = default!;
    public string AddressTypeName { get; set; } = default!;
}
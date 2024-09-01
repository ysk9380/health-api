namespace Health.Api.Models.Patient;

public class PatientPhoneModel
{
    public long PatientPhoneId { get; set; }
    public long PatientId { get; set; }
    public string PhoneNumber { get; set; } = default!;
    public string? ListedAs { get; set; }
    public string PhoneTypeCode { get; set; } = default!;
    public string PhoneTypeName { get; set; } = default!;
}
namespace Health.Api.Models.Requests.Patient;

public class UpdatePatientPhoneRequest
{
    public long PatientPhoneId { get; set; }
    public string PhoneTypeCode { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string ListedAs { get; set; } = default!;
}
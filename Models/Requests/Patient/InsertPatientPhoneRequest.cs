namespace Health.Api.Models.Requests.Patient;

public class InsertPatientPhoneRequest
{
    public long PatientId { get; set; }
    public string PhoneTypeCode { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string ListedAs { get; set; } = default!;
}
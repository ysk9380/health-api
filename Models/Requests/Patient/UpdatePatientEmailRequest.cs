namespace Health.Api.Models.Requests.Patient;

public class UpdatePatientEmailRequest
{
    public long PatientEmailId { get; set; }
    public string EmailAddress { get; set; } = default!;
}
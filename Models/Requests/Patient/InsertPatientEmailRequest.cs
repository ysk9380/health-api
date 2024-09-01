namespace Health.Api.Models.Requests.Patient;

public class InsertPatientEmailRequest
{
    public long PatientId { get; set; }
    public string EmailAddress { get; set; } = default!;
}
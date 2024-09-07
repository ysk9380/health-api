namespace Health.Api.Models.Requests.Patient;

public class InsertPatientHealthVitalRequest
{
    public long PatientId { get; set; }
    public string HealthVitalCode { get; set; } = default!;
    public string Content { get; set; } = default!;
}
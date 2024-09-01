namespace Health.Api.Models.Requests.Patient;

public class UpdatePatientRequest
{
    public long PatientId { get; set; }
    public string Firstname { get; set; } = default!;
    public string Middlename { get; set; } = default!;
    public string Lastname { get; set; } = default!;
    public string GenderCode { get; set; } = default!;
    public DateTime DateOfBirth { get; set; } = default;
}
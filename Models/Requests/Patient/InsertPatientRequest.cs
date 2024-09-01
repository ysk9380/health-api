namespace Health.Api.Models.Requests.Patient;

public class InsertPatientRequest
{
    public string Firstname { get; set; } = default!;
    public string Middlename { get; set; } = default!;
    public string Lastname { get; set; } = default!;
    public string GenderCode { get; set; } = default!;
    public DateTime DateOfBirth { get; set; } = default;
    public bool ForceCreate { get; set; }
}
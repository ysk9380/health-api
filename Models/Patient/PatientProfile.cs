namespace Health.Api.Models.Patient;

public class PatientProfile
{
    public long PatientId { get; set; }
    public string PatientCode { get; set; } = default!;
    public string Firstname { get; set; } = default!;
    public string? Middlename { get; set; } = default!;
    public string Lastname { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public string GenderCode { get; set; } = default!;
    public string GenderName { get; set; } = default!;
}
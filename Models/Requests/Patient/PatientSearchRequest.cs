namespace Health.Api.Models.Requests.Patient;

public class PatientSearchRequest
{
    public string Firstname { get; set; } = default!;
    public string Lastname { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string IdentityNumber { get; set; } = default!;
    public string Email { get; set; } = default!;
}
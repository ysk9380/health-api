using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("Patient")]
public class Patient
{
    public long PatientId { get; set; }
    public string PatientCode { get; set; } = default!;
    public string Firstname { get; set; } = default!;
    public string? Middlename { get; set; } = default!;
    public string Lastname { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public byte GenderId { get; set; }

    public Patient(Patient originalPatient)
    {
        PatientId = originalPatient.PatientId;
        PatientCode = originalPatient.PatientCode;
        Firstname = originalPatient.Firstname;
        Middlename = originalPatient.Middlename;
        Lastname = originalPatient.Lastname;
        DateOfBirth = originalPatient.DateOfBirth;
        GenderId = originalPatient.GenderId;
    }

    public Patient()
    {
    }
}
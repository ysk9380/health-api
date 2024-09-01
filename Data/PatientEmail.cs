using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("PatientEmail")]
public class PatientEmail
{
    public long PatientEmailId { get; set; }
    public long PatientId { get; set; }
    public string EmailAddress { get; set; } = default!;
    public bool IsActive { get; set; }

    public PatientEmail(PatientEmail email)
    {
        PatientEmailId = email.PatientEmailId;
        PatientId = email.PatientId;
        EmailAddress = email.EmailAddress;
        IsActive = email.IsActive;
    }

    public PatientEmail()
    {

    }
}
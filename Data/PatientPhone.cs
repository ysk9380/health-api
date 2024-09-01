using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("PatientPhone")]
public class PatientPhone
{
    public long PatientPhoneId { get; set; }
    public long PatientId { get; set; }
    public byte PhoneTypeId { get; set; }
    public string PhoneNumber { get; set; } = default!;
    public string? ListedAs { get; set; }
    public bool IsActive { get; set; }

    public PatientPhone(PatientPhone phone)
    {
        PatientPhoneId = phone.PatientPhoneId;
        PatientId = phone.PatientId;
        PhoneTypeId = phone.PhoneTypeId;
        PhoneNumber = phone.PhoneNumber;
        ListedAs = phone.ListedAs;
        IsActive = phone.IsActive;
    }

    public PatientPhone()
    {

    }
}
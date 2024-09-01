using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("AppUserPhone")]
public class AppUserPhone
{
    public long AppUserPhoneId { get; set; }
    public long AppUserId { get; set; }
    public byte PhoneTypeId { get; set; }
    public string PhoneNumber { get; set; } = default!;
    public string? ListedAs { get; set; }
    public bool IsActive { get; set; }

    public AppUserPhone(AppUserPhone phone)
    {
        AppUserPhoneId = phone.AppUserPhoneId;
        AppUserId = phone.AppUserId;
        PhoneTypeId = phone.PhoneTypeId;
        PhoneNumber = phone.PhoneNumber;
        ListedAs = phone.ListedAs;
        IsActive = phone.IsActive;
    }

    public AppUserPhone()
    {

    }
}
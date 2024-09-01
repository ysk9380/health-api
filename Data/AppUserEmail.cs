using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("AppUserEmail")]
public class AppUserEmail
{
    public long AppUserEmailId { get; set; }
    public long AppUserId { get; set; }
    public string EmailAddress { get; set; } = default!;
    public bool IsActive { get; set; }

    public AppUserEmail(AppUserEmail email)
    {
        AppUserEmailId = email.AppUserEmailId;
        AppUserId = email.AppUserId;
        EmailAddress = email.EmailAddress;
        IsActive = email.IsActive;
    }

    public AppUserEmail()
    {

    }
}
using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("CustomerAppUser")]
public class CustomerAppUser
{
    public long CustomerAppUserId { get; set; }
    public long CustomerId { get; set; }
    public long AppUserId { get; set; }
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public byte AppUserRoleId { get; set; }
    public byte LanguageId { get; set; }
    public bool IsActive { get; set; }
    public string? Token { get; set; }

    public CustomerAppUser() { }

    public CustomerAppUser(CustomerAppUser copy)
    {
        CustomerAppUserId = copy.CustomerAppUserId;
        CustomerId = copy.CustomerId;
        AppUserId = copy.AppUserId;
        Username = copy.Username;
        Password = copy.Password;
        AppUserRoleId = copy.AppUserRoleId;
        LanguageId = copy.LanguageId;
        IsActive = copy.IsActive;
        Token = copy.Token;
    }
}
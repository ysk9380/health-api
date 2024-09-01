using System.ComponentModel.DataAnnotations.Schema;
namespace Health.Api.Data;

[Table("AppUser")]
public class AppUser
{
    public long AppUserId { get; set; }
    public string Firstname { get; set; } = default!;
    public string? Middlename { get; set; }
    public string Lastname { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public byte GenderId { get; set; }

    public AppUser(AppUser originalAppUser)
    {
        AppUserId = originalAppUser.AppUserId;
        Firstname = originalAppUser.Firstname;
        Middlename = originalAppUser.Middlename;
        Lastname = originalAppUser.Lastname;
        DateOfBirth = originalAppUser.DateOfBirth;
        GenderId = originalAppUser.GenderId;
    }

    public AppUser() { }
}
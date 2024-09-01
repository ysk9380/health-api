namespace Health.Api.Models.User;

public class AppUserModel
{
    public long AppUserId { get; set; }
    public string Firstname { get; set; } = default!;
    public string? Middlename { get; set; }
    public string Lastname { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public byte GenderId { get; set; }
    public string GenderCode { get; set; } = default!;
    public string GenderName { get; set; } = default!;

}
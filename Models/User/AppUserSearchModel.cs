namespace Health.Api.Models.User;

public class AppUserSearchModel
{
    public long AppUserId { get; set; }
    public string Firstname { get; set; } = default!;
    public string? Middlename { get; set; }
    public string Lastname { get; set; } = default!;
    public string Gender { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = default!;
    public string PhoneNumbersString { get; set; } = default!;
    public string EmailAddress { get; set; } = default!;
    public string EmailAddressesString { get; set; } = default!;
    public string IdentityNumber { get; set; } = default!;
    public string IdentityNumbersString { get; set; } = default!;
}
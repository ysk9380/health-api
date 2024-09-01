namespace Health.Api.Models.User;

public class CustomerAppUserModel
{
    public long CustomerAppUserId { get; set; }
    public long CustomerId { get; set; }
    public string? CustomerCode { get; set; } = default!;
    public string? CustomerShortName { get; set; } = default!;
    public string? CustomerName { get; set; } = default!;
    public long AppUserId { get; set; }
    public string Firstname { get; set; } = default!;
    public string? Middlename { get; set; }
    public string Lastname { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public byte GenderId { get; set; }
    public string GenderCode { get; set; } = default!;
    public string GenderName { get; set; } = default!;
    public string? Username { get; set; } = default!;
    public byte AppUserRoleId { get; set; }
    public string? AppUserRoleCode { get; set; } = default!;
    public string? AppUserRoleName { get; set; } = default!;
    public byte LanguageId { get; set; }
    public string? LanguageCode { get; set; } = default!;
    public string? LanguageName { get; set; } = default!;
    public bool IsActive { get; set; }
}
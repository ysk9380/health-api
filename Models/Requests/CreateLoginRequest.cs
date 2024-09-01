namespace Health.Api.Models.Requests;

public class CreateLoginRequest
{
    public long CustomerId { get; set; }
    public long AppUserId { get; set; }
    public string EmailAddress { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string LanguageCode { get; set; } = default!;
    public string RoleCode { get; set; } = default!;
}
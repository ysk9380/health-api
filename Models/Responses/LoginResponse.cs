namespace Health.Api.Models.Responses;

public class LoginResponse
{
    public long AppUserId { get; set; }
    public string Username { get; set; } = default!;
    public string Firstname { get; set; } = default!;
    public string Lastname { get; set; } = default!;
    public long CustomerId { get; set; }
    public string CustomerCode { get; set; } = default!;
    public string CustomerShortName { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string LanguageCode { get; set; } = default!;
    public string Secret { get; set; } = default!;
    public string RoleCode { get; set; } = default!;
}
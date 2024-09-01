namespace Health.Api.Models.Requests;

public class AdminLoginRequest
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}
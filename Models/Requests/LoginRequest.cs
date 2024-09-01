namespace Health.Api.Models.Requests;

public class LoginRequest
{
    public string CustomerCode { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}
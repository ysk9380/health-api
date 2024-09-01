namespace Health.Api.Models.Requests;

public class ChangePasswordRequest
{
    public string CustomerCode { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string OldPassword { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}
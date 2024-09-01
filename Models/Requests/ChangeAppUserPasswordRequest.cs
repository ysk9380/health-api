namespace Health.Api.Models.Requests.User;

public class ChangeAppUserPasswordRequest
{
    public long CustomerAppUserId { get; set; }
    public string NewPassword { get; set; } = default!;
}
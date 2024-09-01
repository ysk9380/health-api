namespace Health.Api.Models.Requests.User;

public class UpdateAppUserEmailRequest
{
    public long AppUserEmailId { get; set; }
    public string EmailAddress { get; set; } = default!;
}
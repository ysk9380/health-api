namespace Health.Api.Models.Requests.User;

public class InsertAppUserEmailRequest
{
    public long AppUserId { get; set; }
    public string EmailAddress { get; set; } = default!;
}
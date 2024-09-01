namespace Health.Api.Models.Requests.User;

public class CustomerAppUserRequest
{
    public long CustomerId { get; set; }
    public long AppUserId { get; set; }
}
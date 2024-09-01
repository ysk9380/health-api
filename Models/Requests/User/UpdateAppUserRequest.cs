namespace Health.Api.Models.Requests.User;

public class UpdateAppUserRequest
{
    public long AppUserId { get; set; }
    public string Firstname { get; set; } = default!;
    public string? Middlename { get; set; }
    public string Lastname { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public string GenderCode { get; set; } = default!;
}
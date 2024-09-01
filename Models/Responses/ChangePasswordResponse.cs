namespace Health.Api.Models.Responses;

public class ChangePasswordResponse
{
    public string Result { get; set; } = default!;
    public bool IsSuccess { get; set; }
}
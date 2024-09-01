namespace Health.Api.Models.Requests.Operational;

public class UpdateAccountRequest
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = default!;
    public string AccountName { get; set; } = default!;
}
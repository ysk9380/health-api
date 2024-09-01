namespace Health.Api.Models.Requests.Operational;

public class InsertAccountRequest
{
    public string AccountCode { get; set; } = default!;
    public string AccountName { get; set; } = default!;
}
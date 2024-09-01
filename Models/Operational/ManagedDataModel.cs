namespace Health.Api.Models.Operational;

public class ManagedDataModel
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Code { get; set; } = default!;
    public int Count { get; set; }
}
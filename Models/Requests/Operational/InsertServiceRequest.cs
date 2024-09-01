namespace Health.Api.Models.Requests.Operational;

public class InsertServiceRequest
{
    public string ServiceCode { get; set; } = default!;
    public string ServiceName { get; set; } = default!;
    public string ServiceCategoryCode { get; set; } = default!;
    public decimal StandardPrice { get; set; }
}
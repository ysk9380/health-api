namespace Health.Api.Models.Requests.Operational;

public class UpdateServiceRequest
{
    public int ServiceId { get; set; }
    public string ServiceCode { get; set; } = default!;
    public string ServiceName { get; set; } = default!;
    public string ServiceCategoryCode { get; set; } = default!;
    public decimal StandardPrice { get; set; }
}
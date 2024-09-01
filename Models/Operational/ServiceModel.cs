namespace Health.Api.Models.Operational;

public class ServiceModel
{
    public int ServiceId { get; set; }
    public string ServiceCode { get; set; } = default!;
    public string ServiceName { get; set; } = default!;
    public byte ServiceCategoryId { get; set; }
    public string ServiceCategoryCode { get; set; } = default!;
    public string ServiceCategoryName { get; set; } = default!;
    public decimal StandardPrice { get; set; }
}
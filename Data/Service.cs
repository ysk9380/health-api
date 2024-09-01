using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("Service")]
public class Service
{
    public int ServiceId { get; set; }
    public string ServiceCode { get; set; } = default!;
    public string ServiceName { get; set; } = default!;
    public byte ServiceCategoryId { get; set; }
    public decimal StandardPrice { get; set; }
}
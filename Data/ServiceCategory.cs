using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("ServiceCategory")]
public class ServiceCategory
{
    public byte ServiceCategoryId { get; set; }
    public string ServiceCategoryCode { get; set; } = default!;
    public string ServiceCategoryName { get; set; } = default!;
}

using System.ComponentModel.DataAnnotations.Schema;
namespace Health.Api.Data;

[Table("Market")]
public class Market
{
    public short MarketId { get; set; }
    public string MarketCode { get; set; } = default!;
    public string MarketName { get; set; } = default!;
    public bool IsActive { get; set; }
}
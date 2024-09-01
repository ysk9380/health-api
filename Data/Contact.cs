using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("Contact")]
public class Contact
{
    public long ContactId { get; set; }
    public int MarketId { get; set; }
    public string Name { get; set; } = default!;
    public string Mobile { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Address { get; set; } = default!;
    public string? Note { get; set; } = default!;
    public bool ArrangeDemo { get; set; }
    public DateTime CreatedTimestamp { get; set; } = default!;
    public bool IsClosed { get; set; }
}
using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("IdentityType")]
public class IdentityType
{
    public byte IdentityTypeId { get; set; }
    public string IdentityTypeCode { get; set; } = default!;
    public string IdentityTypeName { get; set; } = default!;
}
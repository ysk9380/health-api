using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("AddressType")]
public class AddressType
{
    public byte AddressTypeId { get; set; }
    public string AddressTypeCode { get; set; } = default!;
    public string AddressTypeName { get; set; } = default!;
}
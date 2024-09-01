using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("PhoneType")]
public class PhoneType
{
    public byte PhoneTypeId { get; set; }
    public string PhoneTypeCode { get; set; } = default!;
    public string PhoneTypeName { get; set; } = default!;
}
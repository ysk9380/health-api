using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("Gender")]
public class Gender
{
    public byte GenderId { get; set; }
    public string GenderCode { get; set; } = default!;
    public string GenderName { get; set; } = default!;
}
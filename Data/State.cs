using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("State")]
public class State
{
    public byte StateId { get; set; }
    public string StateCode { get; set; } = default!;
    public string StateName { get; set; } = default!;
}
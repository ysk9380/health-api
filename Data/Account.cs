using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("Account")]
public class Account
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = default!;
    public string AccountName { get; set; } = default!;
}
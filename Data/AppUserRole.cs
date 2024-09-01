using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("AppUserRole")]
public class AppUserRole
{
    public byte AppUserRoleId { get; set; }
    public string AppUserRoleCode { get; set; } = default!;
    public string AppUserRoleName { get; set; } = default!;
}
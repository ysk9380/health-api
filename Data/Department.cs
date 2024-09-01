using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("Department")]
public class Department
{
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = default!;
    public string DepartmentName { get; set; } = default!;
}
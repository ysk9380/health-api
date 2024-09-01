namespace Health.Api.Models.Requests.Operational;

public class UpdateDepartmentRequest
{
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = default!;
    public string DepartmentName { get; set; } = default!;
}
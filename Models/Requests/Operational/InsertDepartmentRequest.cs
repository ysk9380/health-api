namespace Health.Api.Models.Requests.Operational;

public class InsertDepartmentRequest
{
    public string DepartmentCode { get; set; } = default!;
    public string DepartmentName { get; set; } = default!;
}
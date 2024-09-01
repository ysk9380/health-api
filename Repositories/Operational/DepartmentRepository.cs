using Health.Api.Data;
using Health.Api.Models.Requests.Operational;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Operational;

public interface IDepartmentRepository
{
    Task<IList<Department>> GetDepartmentsAsync();
    Task<Department?> GetDepartmentByIdAsync(int departmentId);
    Task<Department?> GetDepartmentByCodeAsync(string departmentCode);
    Task<Department> InsertDepartmentAsync(InsertDepartmentRequest request);
    Task<Department> UpdateDepartmentAsync(UpdateDepartmentRequest request);
    Task<int> GetDepartmentsCountAsync();
}

public class DepartmentRepository : IDepartmentRepository
{
    public HASDbContext _Context;

    public DepartmentRepository(HASDbContext context)
    {
        _Context = context;
    }

    public async Task<Department?> GetDepartmentByIdAsync(int departmentId)
    {
        return await _Context.Departments
                        .Where(d => d.DepartmentId.Equals(departmentId))
                        .FirstOrDefaultAsync();
    }

    public async Task<Department?> GetDepartmentByCodeAsync(string departmentCode)
    {
        return await _Context.Departments
                        .Where(d => d.DepartmentCode.Equals(departmentCode))
                        .FirstOrDefaultAsync();
    }

    public async Task<IList<Department>> GetDepartmentsAsync()
    {
        return await _Context.Departments.ToListAsync();
    }

    public async Task<Department> InsertDepartmentAsync(InsertDepartmentRequest request)
    {
        Department newDepartment = new()
        {
            DepartmentCode = request.DepartmentCode,
            DepartmentName = request.DepartmentName
        };

        _Context.Departments.Add(newDepartment);
        await _Context.SaveChangesAsync();
        return newDepartment;
    }

    public async Task<Department> UpdateDepartmentAsync(UpdateDepartmentRequest request)
    {
        Department? existingRecord = await GetDepartmentByIdAsync(request.DepartmentId);
        if (existingRecord == null)
            throw new FileNotFoundException($"Department with id ({request.DepartmentId}) not found.");

        existingRecord.DepartmentCode = request.DepartmentCode;
        existingRecord.DepartmentName = request.DepartmentName;
        _Context.Departments.Update(existingRecord);
        await _Context.SaveChangesAsync();

        return existingRecord;
    }

    public async Task<int> GetDepartmentsCountAsync()
    {
        return await _Context.Departments.CountAsync();
    }
}

using FluentValidation;
using Health.Api.Data;
using Health.Api.Models.Constants;
using Health.Api.Models.Requests.Operational;
using Health.Api.Repositories.Operational;

namespace Health.Api.Services.Operational;

public class DepartmentService
{
    private const string BaseService = "/api/operational/departments";

    internal static async Task<IResult> GetDepartmentsAsync(IDepartmentRepository departmentRepository)
    {
        IList<Department> departments = await departmentRepository.GetDepartmentsAsync();
        return Results.Ok(departments);
    }

    internal static async Task<IResult> GetDepartmentByIdAsync(byte id
        , IDepartmentRepository departmentRepository)
    {
        Department? department = await departmentRepository.GetDepartmentByIdAsync(id);
        return Results.Ok(department);
    }

    internal static async Task<IResult> GetDepartmentByCodeAsync(string code
        , IDepartmentRepository departmentRepository)
    {
        Department? department = await departmentRepository.GetDepartmentByCodeAsync(code);
        return Results.Ok(department);
    }

    internal static async Task<IResult> InsertDepartmentAsync(InsertDepartmentRequest request
        , IValidator<InsertDepartmentRequest> requestValidator
        , IDepartmentRepository departmentRepository
        , ILogger<DepartmentService> logger)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        Department? existingRecord = await departmentRepository.GetDepartmentByCodeAsync(request.DepartmentCode);
        if (existingRecord != null)
        {
            logger.LogError("Department Insertion Conflict. Another department existing with same code {departmentCode}"
                , request.DepartmentCode);
            return Results.Conflict<Department>(existingRecord);
        }

        Department newDepartment = await departmentRepository.InsertDepartmentAsync(request);
        return Results.Created($"api/operational/departments/{newDepartment.DepartmentId}", newDepartment);
    }

    internal static async Task<IResult> UpdateDepartmentAsync(UpdateDepartmentRequest request
        , IValidator<UpdateDepartmentRequest> requestValidator
        , IDepartmentRepository departmentRepository
        , ILogger<DepartmentService> logger)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        Department? existingRecord = await departmentRepository.GetDepartmentByCodeAsync(request.DepartmentCode);
        if (existingRecord != null && existingRecord.DepartmentId != request.DepartmentId)
        {
            logger.LogError("Deparment Update Conflict. Another department existing with same code {departmentCode}"
                , request.DepartmentCode);
            return Results.Conflict<Department>(existingRecord);
        }

        Department updatedDepartment = await departmentRepository.UpdateDepartmentAsync(request);
        return Results.Accepted($"api/operational/departments/{updatedDepartment.DepartmentId}", updatedDepartment);
    }

    public static void Register(WebApplication app)
    {
        app.MapGet(BaseService, GetDepartmentsAsync)
        .WithName("GetDepartments")
        .WithDescription("Returns the list of departments.")
        .Produces<IList<Department>>(200)
        .RequireAuthorization();

        app.MapGet(string.Join("", BaseService, "/{id}"), GetDepartmentByIdAsync)
        .WithName("GetDepartmentById")
        .WithDescription("Returns a department based on id.")
        .Produces<Department>(200)
        .RequireAuthorization();

        app.MapGet(string.Join("", BaseService, "/code/{code}"), GetDepartmentByCodeAsync)
        .WithName("GetDepartmentByCode")
        .WithDescription("Returns a department based on code.")
        .Produces<Department>(200)
        .RequireAuthorization();

        app.MapPost(BaseService, InsertDepartmentAsync)
        .WithName("InsertDepartment")
        .WithDescription("Inserts a new department.")
        .Produces<IList<Department>>(201)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPut(BaseService, UpdateDepartmentAsync)
        .WithName("UpdateDepartment")
        .WithDescription("Updates an existing department.")
        .Produces<IList<Department>>(202)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });
    }
}
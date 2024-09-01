using FluentValidation;
using Health.Api.Models.Operational;
using Health.Api.Repositories.Customers;
using Health.Api.Repositories.Operational;
using Health.Api.Repositories.Users;

namespace Health.Api.Services.Operational;

public class OperationalService
{
    private const string BaseService = "/api/operational";

    internal static async Task<IResult> GetManagedDataSummaryAsync(ICustomerRepository customerRepository
        , IAppUserRepository appUserRepository
        , IAccountRepository accountRepository
        , IDepartmentRepository departmentRepository
        , IMedicalServiceRepository medicalServiceRepository)
    {
        List<ManagedDataModel> managedDataModels = new List<ManagedDataModel>();
        Task<int> GetCustomerCountTask = customerRepository.GetCustomerCountAsync();
        Task<int> GetAppUsersCountTask = appUserRepository.GetAppUsersCountAsync();
        Task<int> GetAccountsCountTask = accountRepository.GetAccountsCountAsync();
        Task<int> GetDepartmentsCountTask = departmentRepository.GetDepartmentsCountAsync();
        Task<int> GetMedicalServicesCountTask = medicalServiceRepository.GetMedicalServicesCountAsync();

        await Task.WhenAll(GetCustomerCountTask
            , GetAppUsersCountTask
            , GetAccountsCountTask
            , GetDepartmentsCountTask
            , GetMedicalServicesCountTask);

        managedDataModels.Add(new ManagedDataModel
        {
            Title = "Customers",
            Code = "customers",
            Description = "Manage customer information.",
            Count = GetCustomerCountTask.Result
        });
        managedDataModels.Add(new ManagedDataModel
        {
            Title = "Application Users",
            Code = "appusers",
            Description = "Manage application users information.",
            Count = GetAppUsersCountTask.Result
        });
        managedDataModels.Add(new ManagedDataModel
        {
            Title = "Accounts",
            Code = "accounts",
            Description = "Manage transaction accounts information.",
            Count = GetAccountsCountTask.Result
        });
        managedDataModels.Add(new ManagedDataModel
        {
            Title = "Departments",
            Code = "departments",
            Description = "Manage departments information.",
            Count = GetDepartmentsCountTask.Result
        });
        managedDataModels.Add(new ManagedDataModel
        {
            Title = "Medical Services",
            Code = "medicalservices",
            Description = "Manage medical services information.",
            Count = GetMedicalServicesCountTask.Result
        });

        return Results.Ok(managedDataModels);
    }

    public static void Register(WebApplication app)
    {
        app.MapGet($"{BaseService}/manageddatasummary", GetManagedDataSummaryAsync)
                .WithName("GetManagedDataSummary")
                .WithDescription("Fetches the summarized information for managed records")
                .Produces<ManagedDataModel[]>(200);

        DepartmentService.Register(app);
        MedicalService.Register(app);
        AccountService.Register(app);
    }
}
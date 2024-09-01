using System.Security.Claims;
using Health.Api.Authentication;
using Health.Api.Data;
using Health.Api.Repositories;

namespace Health.Api.Services;

public static class ReportingService
{
    private const string BaseService = "/api/reports";

    internal static async Task<IResult> GetNewPatientsCountAsync(IReportingRepository reportingRepository
        , ClaimsPrincipal userPrincipal)
    {
        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        int count = await reportingRepository.GetNewPatientsCountAsync(customerId, 7);
        return Results.Ok(count);
    }

    public static void Register(WebApplication app)
    {
        app.MapGet($"{BaseService}/patients/new/count", GetNewPatientsCountAsync)
        .WithName("GetNewPatientsCount")
        .WithDescription("Retrieves the count of new patients registered in last 7 days for a customer.")
        .Produces<int>(200)
        .RequireAuthorization();
    }
}
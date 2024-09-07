using System.Security.Claims;
using FluentValidation;
using Health.Api.Authentication;
using Health.Api.Data;
using Health.Api.Models.Hospitals;
using Health.Api.Models.Requests.Hospitals;
using Health.Api.Repositories.Hospitals;

namespace Health.Api.Services.Hospitals;

public class AppointmentService
{
    private const string BaseService = "/api/hospitals/appointments";
    private const string AppointmentTableName = "appointment";

    internal static async Task<IResult> GetAppointmentDetailsByDateAsync(AppointmentDateRequest request
            , IAppointmentRepository appointmentRepository
            , ClaimsPrincipal userPrincipal)
    {
        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        IList<AppointmentDetail> appointments
            = await appointmentRepository.GetAppointmentDetailsByDateAsync(customerId, request.DateOfAppointment);
        return Results.Ok(appointments);
    }

    public static void Register(WebApplication app)
    {
        app.MapPost(string.Join("/", BaseService), GetAppointmentDetailsByDateAsync)
        .WithName("GetAppointmentDetailsByDate")
        .WithDescription("Get appointment details by date.")
        .Produces<PatientHealthVital>(200)
        .Produces(401)
        .RequireAuthorization();
    }
}
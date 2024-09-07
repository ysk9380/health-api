using System.Security.Claims;
using FluentValidation;
using Health.Api.Authentication;
using Health.Api.Data;
using Health.Api.Models.Requests.Patient;
using Health.Api.Repositories;
using Health.Api.Repositories.Patients;

namespace Health.Api.Services.Patients;
public class PatientHealthVitalService
{
    private const string BaseService = "/api/patients/vitals";
    private const string PatientHealthVitalTableName = "patienthealthvital";

    internal static async Task<IResult> GetPatientPatientHealthVitalByIdAsync(long id
            , IPatientHealthVitalRepository patientHealthVitalRepository)
    {
        PatientHealthVital? patientHealthVital = await patientHealthVitalRepository.GetPatientHealthVitalById(id);
        return Results.Ok(patientHealthVital);
    }

    internal static async Task<IResult> GetPatientHealthVitalsByPatientIdAsync(long patientId
            , IPatientHealthVitalRepository patientHealthVitalRepository)
    {
        IList<PatientHealthVital> patientHealthVitals = await patientHealthVitalRepository.GetPatientHealthVitalsAsync(id);
        return Results.Ok(patientHealthVitals);
    }

    internal static async Task<IResult> InsertPatientHealthVitalAsync(InsertPatientHealthVitalRequest request
            , IValidator<InsertPatientHealthVitalRequest> requestValidator
            , IMasterRepository masterRepository
            , IPatientHealthVitalRepository patientHealthVitalRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        int healthVitalId = await masterRepository.GetHealthVitalIdAsync(request.HealthVitalCode);
        if (healthVitalId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_HEALTH_VITAL_CODE",
                Note = $"Vital Type ({request.HealthVitalCode}) is unrecognized."
            });

        PatientHealthVital newPatientHealthVital
            = await patientHealthVitalRepository.InsertNewPatientHealthVitalAsync(request
            , healthVitalId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordNewModelAuditDataAsync<PatientHealthVital>(newPatientHealthVital
                , PatientHealthVitalTableName
                , newPatientHealthVital.PatientHealthVitalId
                , customerId
                , appUserId);

        return Results.Created($"{BaseService}/{newPatientHealthVital.PatientHealthVitalId}", newPatientHealthVital);
    }

    internal static async Task<IResult> DeactivatePatientHealthVitalAsync(long id
            , IPatientHealthVitalRepository patientHealthVitalRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        await patientHealthVitalRepository.DeactivatePatientEmailAsync(id);
        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordDeactivatedAuditDataAsync(PatientHealthVitalTableName
                , id
                , customerId
                , appUserId);
        return Results.NoContent();
    }

    public static void Register(WebApplication app)
    {
        app.MapGet(string.Join("/", BaseService, "{id}"), GetPatientPatientHealthVitalByIdAsync)
        .WithName("GetPatientEmailById")
        .WithDescription("Fetches patient email record using patient email id.")
        .Produces<PatientHealthVital>(200)
        .Produces(401)
        .RequireAuthorization();

        app.MapGet(string.Join("/", BaseService, "patient/{id}"), GetPatientHealthVitalsByPatientIdAsync)
        .WithName("GetPatientEmailsByPatientId")
        .WithDescription("Fetches patient email records using patient id.")
        .Produces<PatientEmail[]>(200)
        .Produces(401)
        .RequireAuthorization();

        app.MapPost($"{BaseService}", InsertPatientHealthVitalAsync)
        .WithName("InsertPatientHealthVital")
        .WithDescription("Inserts a new patient health vital record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces(401)
        .Produces<PatientHealthVital>(201)
        .RequireAuthorization();

        app.MapDelete(string.Join("/", BaseService, "{id}"), DeactivatePatientHealthVitalAsync)
        .WithName("DeactivatePatientHealthVital")
        .WithDescription("Deactivate a patient health vital record.")
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<PatientHealthVital>(204)
        .RequireAuthorization();
    }
}

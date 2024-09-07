using System.Security.Claims;
using FluentValidation;
using Health.Api.Authentication;
using Health.Api.Data;
using Health.Api.Models.Requests.Patient;
using Health.Api.Repositories;
using Health.Api.Repositories.Patients;

namespace Health.Api.Services.Patients;

public class PatientEmailService
{
    private const string BaseService = "/api/patients/emails";
    private const string PatientEmailTableName = "patientemail";

    internal static async Task<IResult> GetPatientEmailByIdAsync(long id
            , IPatientEmailRepository patientEmailRepository)
    {
        PatientEmail? patientEmail = await patientEmailRepository.GetPatientEmailByIdAsync(id);
        return Results.Ok(patientEmail);
    }

    internal static async Task<IResult> GetPatientEmailsByPatientIdAsync(long id
            , IPatientEmailRepository patientEmailRepository)
    {
        IList<PatientEmail> patientEmails = await patientEmailRepository.GetPatientEmailsByPatientIdAsync(id);
        return Results.Ok(patientEmails);
    }

    internal static async Task<IResult> InsertPatientEmailAsync(InsertPatientEmailRequest request
            , IValidator<InsertPatientEmailRequest> requestValidator
            , IPatientEmailRepository patientEmailRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var existingRecord = await patientEmailRepository.GetPatientEmailByEmailAddressAsync(request.PatientId, request.EmailAddress);
        if (existingRecord != null)
            return Results.Conflict<PatientEmail>(existingRecord);

        PatientEmail newPatientEmail = await patientEmailRepository.InsertNewPatientEmailAsync(request);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordNewModelAuditDataAsync<PatientEmail>(newPatientEmail
                , PatientEmailTableName
                , newPatientEmail.PatientEmailId
                , customerId
                , appUserId);

        return Results.Created($"{BaseService}/{newPatientEmail.PatientEmailId}", newPatientEmail);
    }

    internal static async Task<IResult> UpdatePatientEmailAsync(UpdatePatientEmailRequest request
            , IValidator<UpdatePatientEmailRequest> requestValidator
            , IPatientEmailRepository patientEmailRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var existingRecord = await patientEmailRepository.GetPatientEmailByIdAsync(request.PatientEmailId);
        if (existingRecord == null)
            return Results.BadRequest(new
            {
                Code = "PATIENT_EMAIL_RECORD_NOT_FOUND",
                Note = $"Patient email with id ({request.PatientEmailId}) does not exist."
            });
        var existingRecordCopy = new PatientEmail(existingRecord);

        var matchingOtherEmailRecord = await patientEmailRepository.GetPatientEmailByEmailAddressAsync(existingRecord.PatientId
            , request.EmailAddress);
        if (matchingOtherEmailRecord != null &&
            matchingOtherEmailRecord.PatientEmailId != request.PatientEmailId)
            return Results.Conflict<PatientEmail>(matchingOtherEmailRecord);

        PatientEmail updatedPatientEmail = await patientEmailRepository.UpdatePatientEmailAsync(request);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordEditModelAuditDataAsync<PatientEmail>(existingRecordCopy
                , updatedPatientEmail
                , PatientEmailTableName
                , updatedPatientEmail.PatientEmailId
                , customerId
                , appUserId);

        return Results.Accepted($"{BaseService}/{updatedPatientEmail.PatientEmailId}", updatedPatientEmail);
    }

    internal static async Task<IResult> DeactivatePatientEmailAsync(long id
            , IPatientEmailRepository patientEmailRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        await patientEmailRepository.DeactivatePatientEmailAsync(id);
        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordDeactivatedAuditDataAsync(PatientEmailTableName
                , id
                , customerId
                , appUserId);
        return Results.NoContent();
    }

    public static void Register(WebApplication app)
    {
        app.MapGet(string.Join("/", BaseService, "{id}"), GetPatientEmailByIdAsync)
        .WithName("GetPatientEmailById")
        .WithDescription("Fetches patient email record using patient email id.")
        .Produces<PatientEmail>(200)
        .Produces(401)
        .RequireAuthorization();

        app.MapGet(string.Join("/", BaseService, "patient/{id}"), GetPatientEmailsByPatientIdAsync)
        .WithName("GetPatientEmailsByPatientId")
        .WithDescription("Fetches patient email records using patient id.")
        .Produces<PatientEmail[]>(200)
        .Produces(401)
        .RequireAuthorization();

        app.MapPost($"{BaseService}", InsertPatientEmailAsync)
        .WithName("InsertPatientEmail")
        .WithDescription("Inserts a new patient email record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces(401)
        .Produces<PatientEmail>(409)
        .Produces<PatientEmail>(201)
        .RequireAuthorization();

        app.MapPut($"{BaseService}", UpdatePatientEmailAsync)
        .WithName("UpdatePatientEmail")
        .WithDescription("Inserts a new patient email record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces(401)
        .Produces<PatientEmail>(409)
        .Produces<PatientEmail>(201)
        .RequireAuthorization();

        app.MapDelete(string.Join("/", BaseService, "{id}"), DeactivatePatientEmailAsync)
        .WithName("DeactivatePatientEmail")
        .WithDescription("Deactivate a patient email record.")
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<PatientEmail>(204)
        .RequireAuthorization();
    }
}
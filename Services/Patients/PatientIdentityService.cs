using System.Security.Claims;
using FluentValidation;
using Health.Api.Authentication;
using Health.Api.Data;
using Health.Api.Models.Patient;
using Health.Api.Models.Requests.Patient;
using Health.Api.Repositories;
using Health.Api.Repositories.Patients;

namespace Health.Api.Services.Patients;

public class PatientIdentityService
{
    private const string BaseService = "/api/patients/identities";
    private const string PatientIdentityTableName = "patientidentity";

    internal static async Task<IResult> InsertPatientIdentityAsync(InsertPatientIdentityRequest request
            , IValidator<InsertPatientIdentityRequest> requestValidator
            , IMasterRepository masterRepository
            , IPatientIdentityRepository patientIdentityRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        byte identityTypeId = await masterRepository.GetIdentityTypeIdAsync(request.IdentityTypeCode);
        if (identityTypeId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_IDENTITY_TYPE_ID",
                Note = $"Identity Type ({request.IdentityTypeCode}) is unrecognized."
            });

        var existingRecord = await patientIdentityRepository.GetPatientIdentityByIdentityTypeAndNumberAsync(request.PatientId
            , identityTypeId, request.IdentityNumber);
        if (existingRecord != null)
            return Results.Conflict<PatientIdentity>(existingRecord);

        PatientIdentity newPatientIdentity = await patientIdentityRepository.InsertNewPatientIdentityAsync(request, identityTypeId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordNewModelAuditDataAsync<PatientIdentity>(newPatientIdentity
                , PatientIdentityTableName
                , newPatientIdentity.PatientIdentityId
                , customerId
                , appUserId);
        return Results.Created($"{BaseService}/{newPatientIdentity.PatientIdentityId}", newPatientIdentity);
    }

    internal static async Task<IResult> UpdatePatientIdentityAsync(UpdatePatientIdentityRequest request
            , IValidator<UpdatePatientIdentityRequest> requestValidator
            , IMasterRepository masterRepository
            , IPatientIdentityRepository patientIdentityRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        PatientIdentity? existingRecord = await patientIdentityRepository.GetPatientIdentityByIdAsync(request.PatientIdentityId);
        if (existingRecord == null)
            return Results.BadRequest(new
            {
                Code = "PATIENT_IDENTITY_RECORD_NOT_FOUND",
                Note = $"Patient email with id ({request.PatientIdentityId}) does not exist."
            });
        var existingRecordCopy = new PatientIdentity(existingRecord);

        byte identityTypeId = await masterRepository.GetIdentityTypeIdAsync(request.IdentityTypeCode);
        if (identityTypeId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_IDENTITY_TYPE_ID",
                Note = $"Identity Type ({request.IdentityTypeCode}) is unrecognized."
            });

        var matchingOtherIdentityRecord = await patientIdentityRepository.GetPatientIdentityByIdentityTypeAndNumberAsync(existingRecord.PatientId
            , identityTypeId, request.IdentityNumber);
        if (matchingOtherIdentityRecord != null
            && matchingOtherIdentityRecord.PatientIdentityId != existingRecord.PatientIdentityId)
            return Results.Conflict<PatientIdentity>(existingRecord);

        PatientIdentity updatedPatientIdentity = await patientIdentityRepository.UpdatePatientIdentityAsync(request, identityTypeId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordEditModelAuditDataAsync<PatientIdentity>(existingRecordCopy
                , updatedPatientIdentity
                , PatientIdentityTableName
                , updatedPatientIdentity.PatientIdentityId
                , customerId
                , appUserId);

        return Results.Accepted($"{BaseService}/{updatedPatientIdentity.PatientIdentityId}", updatedPatientIdentity);
    }

    internal static async Task<IResult> DeletePatientIdentityAsync(long id
            , IPatientIdentityRepository patientIdentityRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        await patientIdentityRepository.DeactivatePatientIdentityAsync(id);
        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordDeactivatedAuditDataAsync(PatientIdentityTableName
                , id
                , customerId
                , appUserId);
        return Results.NoContent();
    }

    internal static async Task<IResult> GetPatientIdentityByIdAsync(long id
        , IPatientIdentityRepository patientIdentityRepository)
    {
        PatientIdentityModel? patientIdentity = await patientIdentityRepository.GetPatientIdentityModelByIdAsync(id);
        return Results.Ok(patientIdentity);
    }

    internal static async Task<IResult> GetPatientIdentitiesByPatientIdAsync(long id
            , IPatientIdentityRepository patientIdentityRepository)
    {
        IList<PatientIdentityModel> identities = await patientIdentityRepository.GetPatientIdentityModelsByPatientIdAsync(id);
        return Results.Ok(identities);
    }

    public static void Register(WebApplication app)
    {
        app.MapPost($"{BaseService}", InsertPatientIdentityAsync)
        .WithName("InsertPatientIdentity")
        .WithDescription("Inserts a new patient identity record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<PatientIdentity>(409)
        .Produces<PatientIdentity>(201)
        .RequireAuthorization();

        app.MapPut($"{BaseService}", UpdatePatientIdentityAsync)
        .WithName("UpdatePatientIdentity")
        .WithDescription("Updates a patient identity record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<PatientIdentity>(409)
        .Produces<PatientIdentity>(202)
        .RequireAuthorization();

        app.MapDelete(string.Join("/", BaseService, "{id}"), DeletePatientIdentityAsync)
        .WithName("DeactivatePatientIdentity")
        .WithDescription("Deactivate a patient identity record.")
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<PatientAddress>(204)
        .RequireAuthorization();

        app.MapGet(string.Join("/", BaseService, "{id}"), GetPatientIdentityByIdAsync)
        .WithName("GetPatientIdentityById")
        .WithDescription("Fetches patient identity record using patient identity id.")
        .Produces<PatientIdentityModel>(200)
        .Produces(401)
        .RequireAuthorization();

        app.MapGet(string.Join("/", BaseService, "patient/{id}"), GetPatientIdentitiesByPatientIdAsync)
        .WithName("GetPatientIdentitiesByPatientId")
        .WithDescription("Fetches patient identity records using patient id.")
        .Produces<PatientIdentityModel[]>(200)
        .Produces(401)
        .RequireAuthorization();
    }
}
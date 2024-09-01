using System.Security.Claims;
using FluentValidation;
using Health.Api.Authentication;
using Health.Api.Data;
using Health.Api.Models.Patient;
using Health.Api.Models.Requests.Patient;
using Health.Api.Repositories;
using Health.Api.Repositories.Patients;

namespace Health.Api.Services.Patients;

public class PatientPhoneService
{
    private const string BaseService = "/api/patients/phones";
    private const string PatientPhoneTableName = "patientphone";

    internal static async Task<IResult> GetPatientPhoneModelByIdAsync(long id
            , IPatientPhoneRepository patientPhoneRepository)
    {
        PatientPhoneModel? patientPhoneModel = await patientPhoneRepository.GetPatientPhoneModelByIdAsync(id);
        return Results.Ok(patientPhoneModel);
    }

    internal static async Task<IResult> GetPatientPhoneModelsByPatientIdAsync(long id
            , IPatientPhoneRepository patientPhoneRepository)
    {
        IList<PatientPhoneModel> patientPhoneModels = await patientPhoneRepository.GetPatientPhoneModelsByPatientIdAsync(id);
        return Results.Ok(patientPhoneModels);
    }

    internal static async Task<IResult> InsertPatientPhoneAsync(InsertPatientPhoneRequest request
            , IValidator<InsertPatientPhoneRequest> requestValidator
            , IMasterRepository masterRepository
            , IPatientPhoneRepository patientPhoneRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var existingRecord = await patientPhoneRepository.GetPatientPhoneByPhoneNumberAsync(
                                request.PatientId
                                , request.PhoneNumber);
        if (existingRecord != null)
            return Results.Conflict<PatientPhone>(existingRecord);

        byte phoneTypeId = await masterRepository.GetPhoneTypeIdAsync(request.PhoneTypeCode);
        if (phoneTypeId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_PHONE_TYPE_CODE",
                Note = $"Phone type code ({request.PhoneTypeCode}) is unrecognized."
            });

        PatientPhone newPatientPhone = await patientPhoneRepository.InsertNewPatientPhoneAsync(request
            , phoneTypeId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordNewModelAuditDataAsync<PatientPhone>(newPatientPhone
                , PatientPhoneTableName
                , newPatientPhone.PatientPhoneId
                , customerId
                , appUserId);

        return Results.Created($"{BaseService}/{newPatientPhone.PatientPhoneId}", newPatientPhone);
    }

    internal static async Task<IResult> UpdatePatientPhoneAsync(UpdatePatientPhoneRequest request
            , IValidator<UpdatePatientPhoneRequest> requestValidator
            , IMasterRepository masterRepository
            , IPatientPhoneRepository patientPhoneRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var existingRecord = await patientPhoneRepository.GetPatientPhoneByIdAsync(request.PatientPhoneId);
        if (existingRecord == null)
            return Results.BadRequest(new
            {
                Code = "PATIENT_PHONE_RECORD_NOT_FOUND",
                Note = $"Patient phone with id ({request.PatientPhoneId}) does not exist."
            });
        var existingRecordCopy = new PatientPhone(existingRecord);

        var matchingOtherPatientPhone = await patientPhoneRepository.GetPatientPhoneByPhoneNumberAsync(existingRecord.PatientId
            , request.PhoneNumber);
        if (matchingOtherPatientPhone != null
            && matchingOtherPatientPhone.PatientPhoneId != request.PatientPhoneId)
            return Results.Conflict<PatientPhone>(matchingOtherPatientPhone);

        byte phoneTypeId = await masterRepository.GetPhoneTypeIdAsync(request.PhoneTypeCode);
        if (phoneTypeId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_PHONE_TYPE_CODE",
                Note = $"Phone type code ({request.PhoneTypeCode}) is unrecognized."
            });

        PatientPhone updatedPatientPhone = await patientPhoneRepository.UpdatePatientPhoneAsync(request, phoneTypeId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordEditModelAuditDataAsync<PatientPhone>(existingRecordCopy
                , updatedPatientPhone
                , PatientPhoneTableName
                , updatedPatientPhone.PatientPhoneId
                , customerId
                , appUserId);

        return Results.Accepted($"{BaseService}/{updatedPatientPhone.PatientPhoneId}", updatedPatientPhone);
    }

    internal static async Task<IResult> DeletePatientPhoneAsync(long id
            , IPatientPhoneRepository patientPhoneRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        await patientPhoneRepository.DeactivatePatientPhoneAsync(id);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordDeactivatedAuditDataAsync(PatientPhoneTableName
                , id
                , customerId
                , appUserId);
        return Results.NoContent();
    }

    public static void Register(WebApplication app)
    {
        app.MapGet(string.Join("/", BaseService, "{id}"), GetPatientPhoneModelByIdAsync)
        .WithName("GetPatientPhoneById")
        .WithDescription("Fetches patient phone record using patient phone id.")
        .Produces<PatientPhoneModel>(200)
        .Produces(401)
        .RequireAuthorization();

        app.MapGet(string.Join("/", BaseService, "patient/{id}"), GetPatientPhoneModelsByPatientIdAsync)
        .WithName("GetPatientPhoneByPatientId")
        .WithDescription("Fetches patient phone records using patient id.")
        .Produces<PatientPhoneModel[]>(200)
        .Produces(401)
        .RequireAuthorization();

        app.MapPost($"{BaseService}", InsertPatientPhoneAsync)
        .WithName("InsertPatientPhone")
        .WithDescription("Inserts a new patient phone record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces(401)
        .Produces<PatientPhone>(409)
        .Produces<PatientPhone>(201)
        .RequireAuthorization();

        app.MapPut($"{BaseService}", UpdatePatientPhoneAsync)
        .WithName("UpdatePatientPhone")
        .WithDescription("Inserts a new patient phone record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces(401)
        .Produces<PatientPhone>(409)
        .Produces<PatientPhone>(202)
        .RequireAuthorization();

        app.MapDelete(string.Join("/", BaseService, "{id}"), DeletePatientPhoneAsync)
        .WithName("DeactivatePatientPhone")
        .WithDescription("Deactivate a patient phone record.")
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces(204)
        .RequireAuthorization();
    }
}
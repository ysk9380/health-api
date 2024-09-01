using System.Security.Claims;
using FluentValidation;
using Health.Api.Authentication;
using Health.Api.Data;
using Health.Api.Models.Patient;
using Health.Api.Models.Requests.Patient;
using Health.Api.Repositories;
using Health.Api.Repositories.Patients;

namespace Health.Api.Services.Patients;

public class PatientAddressService
{
    private const string BaseService = "/api/patients/addresses";
    private const string PatientAddressTableName = "patientaddress";

    internal static async Task<IResult> GetPatientAddressModelByIdAsync(long id
            , IPatientAddressRepository patientAddressRepository)
    {
        PatientAddressModel? patientAddress = await patientAddressRepository.GetPatientAddressModelByIdAsync(id);
        return Results.Ok(patientAddress);
    }

    internal static async Task<IResult> GetPatientAddressesByPatientIdAsync(long id
            , IPatientAddressRepository patientAddressRepository)
    {
        IList<PatientAddressModel> addresses = await patientAddressRepository.GetPatientAddressModelsByPatientIdAsync(id);
        return Results.Ok(addresses);
    }

    internal static async Task<IResult> InsertPatientAddressAsync(InsertPatientAddressRequest request
            , IValidator<InsertPatientAddressRequest> requestValidator
            , IMasterRepository masterRepository
            , IPatientAddressRepository patientAddressRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        byte addressTypeId = await masterRepository.GetAddressTypeIdAsync(request.AddressTypeCode);
        if (addressTypeId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_ADDRESS_TYPE_CODE",
                Note = $"Address type code ({request.AddressTypeCode}) is unrecognized."
            });

        byte stateId = await masterRepository.GetStateIdAsync(request.StateCode);
        if (stateId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_STATE_CODE",
                Note = $"State code ({request.StateCode}) is unrecognized."
            });

        var existingRecord = await patientAddressRepository.GetPatientAddressByAddressTypeAsync(
            request.PatientId
            , addressTypeId);
        if (existingRecord != null)
            return Results.Conflict<PatientAddress>(existingRecord);

        PatientAddress newPatientAddress = await patientAddressRepository.InsertNewPatientAddressAsync(
            request
            , addressTypeId
            , stateId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordNewModelAuditDataAsync<PatientAddress>(newPatientAddress
                , PatientAddressTableName
                , newPatientAddress.PatientAddressId
                , customerId
                , appUserId);

        return Results.Created($"{BaseService}/{newPatientAddress.PatientAddressId}", newPatientAddress);
    }

    internal static async Task<IResult> UpdatePatientAddressAsync(UpdatePatientAddressRequest request
            , IValidator<UpdatePatientAddressRequest> requestValidator
            , IMasterRepository masterRepository
            , IPatientAddressRepository patientAddressRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        byte addressTypeId = await masterRepository.GetAddressTypeIdAsync(request.AddressTypeCode);
        if (addressTypeId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_ADDRESS_TYPE_CODE",
                Note = $"Address type code ({request.AddressTypeCode}) is unrecognized."
            });

        byte stateId = await masterRepository.GetStateIdAsync(request.StateCode);
        if (stateId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_STATE_CODE",
                Note = $"State code ({request.StateCode}) is unrecognized."
            });

        var existingRecord = await patientAddressRepository.GetPatientAddressByIdAsync(
            request.PatientAddressId);
        if (existingRecord == null)
            return Results.BadRequest(new
            {
                Code = "PATIENT_ADDRESS_RECORD_NOT_FOUND",
                Note = $"Patient Address with id ({request.PatientAddressId}) does not exist."
            });

        PatientAddress? matchingAddressTypeAddress
            = await patientAddressRepository.GetPatientAddressByAddressTypeAsync(existingRecord.PatientId
                , addressTypeId);
        if (matchingAddressTypeAddress != null
            && matchingAddressTypeAddress.PatientAddressId != request.PatientAddressId)
            return Results.Conflict(matchingAddressTypeAddress);

        var existingRecordCopy = new PatientAddress(existingRecord);
        PatientAddress updatedPatientAddress
            = await patientAddressRepository.UpdatePatientAddressRecordAsync(request
                , addressTypeId
                , stateId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordEditModelAuditDataAsync<PatientAddress>(existingRecordCopy
                , updatedPatientAddress
                , PatientAddressTableName
                , updatedPatientAddress.PatientAddressId
                , customerId
                , appUserId);

        return Results.Accepted($"{BaseService}/{updatedPatientAddress.PatientAddressId}", existingRecord);
    }

    internal static async Task<IResult> DeactivatePatientAddressAsync(long id
            , IPatientAddressRepository patientAddressRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        await patientAddressRepository.DeactivatePatientAddressAsync(id);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordDeactivatedAuditDataAsync(PatientAddressTableName
                , id
                , customerId
                , appUserId);

        return Results.NoContent();
    }

    public static void Register(WebApplication app)
    {
        app.MapGet(string.Join("/", BaseService, "{id}"), GetPatientAddressModelByIdAsync)
        .WithName("GetPatientAddressById")
        .WithDescription("Fetches patient address record using patient address id.")
        .Produces<PatientAddressModel>(200)
        .Produces(401)
        .RequireAuthorization();

        app.MapGet(string.Join("/", BaseService, "patient/{id}"), GetPatientAddressesByPatientIdAsync)
        .WithName("GetPatientAddressesByPatientId")
        .WithDescription("Fetches patient address records using patient id.")
        .Produces<PatientAddressModel[]>(200)
        .RequireAuthorization();

        app.MapPost($"{BaseService}", InsertPatientAddressAsync)
        .WithName("InsertPatientAddress")
        .WithDescription("Inserts a new patient address record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<PatientAddress>(409)
        .Produces<PatientAddress>(201)
        .RequireAuthorization();

        app.MapPut($"{BaseService}", UpdatePatientAddressAsync)
        .WithName("UpdatePatientAddress")
        .WithDescription("Inserts a new patient address record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<PatientAddress>(202)
        .RequireAuthorization();

        app.MapDelete(string.Join("/", BaseService, "{id}"), DeactivatePatientAddressAsync)
        .WithName("DeactivatePatientAddress")
        .WithDescription("Deactivate a patient address record.")
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<PatientAddress>(204)
        .RequireAuthorization();
    }
}
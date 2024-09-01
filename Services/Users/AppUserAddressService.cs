using System.Security.Claims;
using FluentValidation;
using Health.Api.Authentication;
using Health.Api.Data;
using Health.Api.Models.Constants;
using Health.Api.Models.Requests.User;
using Health.Api.Models.User;
using Health.Api.Repositories;
using Health.Api.Repositories.Users;

namespace Health.Api.Services.Users;

public class AppUserAddressService
{
    private const string BaseService = "/api/users/addresses";
    private const string AppUserAddressTableName = "appuseraddress";

    internal static async Task<IResult> GetAppUserAddressModelByIdAsync(long id
            , IAppUserAddressRepository appUserAddressRepository)
    {
        AppUserAddressModel? appUserAddress = await appUserAddressRepository.GetAppUserAddressModelByIdAsync(id);
        return Results.Ok(appUserAddress);
    }

    internal static async Task<IResult> GetAppUserAddressesByAppUserIdAsync(long id
            , IAppUserAddressRepository appUserAddressRepository)
    {
        IList<AppUserAddressModel> addresses = await appUserAddressRepository.GetAppUserAddressModelsByAppUserIdAsync(id);
        return Results.Ok(addresses);
    }

    internal static async Task<IResult> InsertAppUserAddressAsync(InsertAppUserAddressRequest request
            , IValidator<InsertAppUserAddressRequest> requestValidator
            , IMasterRepository masterRepository
            , IAppUserAddressRepository appUserAddressRepository
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

        var existingRecord = await appUserAddressRepository.GetAppUserAddressByAddressTypeAsync(
            request.AppUserId
            , addressTypeId);
        if (existingRecord != null)
            return Results.Conflict<AppUserAddress>(existingRecord);

        AppUserAddress newAppUserAddress = await appUserAddressRepository.InsertAppUserAddressAsync(
            request
            , addressTypeId
            , stateId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordNewModelAuditDataAsync<AppUserAddress>(newAppUserAddress
                , AppUserAddressTableName
                , newAppUserAddress.AppUserAddressId
                , customerId
                , appUserId);

        return Results.Created($"{BaseService}/{newAppUserAddress.AppUserAddressId}", newAppUserAddress);
    }

    internal static async Task<IResult> UpdateAppUserAddressAsync(UpdateAppUserAddressRequest request
            , IValidator<UpdateAppUserAddressRequest> requestValidator
            , IMasterRepository masterRepository
            , IAppUserAddressRepository appUserAddressRepository
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

        var existingRecord = await appUserAddressRepository.GetAppUserAddressByIdAsync(
            request.AppUserAddressId);
        if (existingRecord == null)
            return Results.BadRequest(new
            {
                Code = "APP_USER_ADDRESS_RECORD_NOT_FOUND",
                Note = $"App User Address with id ({request.AppUserAddressId}) does not exist."
            });

        AppUserAddress? matchingAddressTypeAddress
            = await appUserAddressRepository.GetAppUserAddressByAddressTypeAsync(existingRecord.AppUserId
                , addressTypeId);
        if (matchingAddressTypeAddress != null
            && matchingAddressTypeAddress.AppUserAddressId != request.AppUserAddressId)
            return Results.Conflict(matchingAddressTypeAddress);

        var existingRecordCopy = new AppUserAddress(existingRecord);
        AppUserAddress updatedAppUserAddress
            = await appUserAddressRepository.UpdateAppUserAddressRecordAsync(request
                , addressTypeId
                , stateId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordEditModelAuditDataAsync<AppUserAddress>(existingRecordCopy
                , updatedAppUserAddress
                , AppUserAddressTableName
                , updatedAppUserAddress.AppUserAddressId
                , customerId
                , appUserId);

        return Results.Accepted($"{BaseService}/{updatedAppUserAddress.AppUserAddressId}", existingRecord);
    }

    internal static async Task<IResult> DeactivateAppUserAddressAsync(long id
            , IAppUserAddressRepository appUserAddressRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        await appUserAddressRepository.DeactivateAppUserAddressAsync(id);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordDeactivatedAuditDataAsync(AppUserAddressTableName
                , id
                , customerId
                , appUserId);

        return Results.NoContent();
    }

    public static void Register(WebApplication app)
    {
        app.MapGet(string.Join("/", BaseService, "{id}"), GetAppUserAddressModelByIdAsync)
        .WithName("GetAppUserAddressById")
        .WithDescription("Fetches app user address record using app user address id.")
        .Produces<AppUserAddressModel>(200)
        .Produces(401)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapGet(string.Join("/", BaseService, "appuser/{id}"), GetAppUserAddressesByAppUserIdAsync)
        .WithName("GetAppUserAddressesByAppUserId")
        .WithDescription("Fetches app user address records using app user id.")
        .Produces<AppUserAddressModel[]>(200)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPost($"{BaseService}", InsertAppUserAddressAsync)
        .WithName("InsertAppUserAddress")
        .WithDescription("Inserts a new app user address record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<AppUserAddress>(409)
        .Produces<AppUserAddress>(201)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPut($"{BaseService}", UpdateAppUserAddressAsync)
        .WithName("UpdateAppUserAddress")
        .WithDescription("Inserts a new app user address record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<AppUserAddress>(202)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapDelete(string.Join("/", BaseService, "{id}"), DeactivateAppUserAddressAsync)
        .WithName("DeactivateAppUserAddress")
        .WithDescription("Deactivate a app user address record.")
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces(204)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });
    }
}
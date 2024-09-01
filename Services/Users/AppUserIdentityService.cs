using System.Security.Claims;
using FluentValidation;
using Health.Api.Authentication;
using Health.Api.Data;
using Health.Api.Models.User;
using Health.Api.Models.Requests.User;
using Health.Api.Repositories;
using Health.Api.Repositories.Users;
using Health.Api.Models.Constants;

namespace Health.Api.Services.Users;

public class AppUserIdentityService
{
    private const string BaseService = "/api/users/identities";
    private const string AppUserIdentityTableName = "appuseridentity";

    internal static async Task<IResult> InsertAppUserIdentityAsync(InsertAppUserIdentityRequest request
            , IValidator<InsertAppUserIdentityRequest> requestValidator
            , IMasterRepository masterRepository
            , IAppUserIdentityRepository appUserIdentityRepository
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

        var existingRecord = await appUserIdentityRepository.GetAppUserIdentityByIdentityTypeAndNumberAsync(request.AppUserId
            , identityTypeId, request.IdentityNumber);
        if (existingRecord != null)
            return Results.Conflict<AppUserIdentity>(existingRecord);

        AppUserIdentity newAppUserIdentity = await appUserIdentityRepository.InsertAppUserIdentityAsync(request, identityTypeId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordNewModelAuditDataAsync<AppUserIdentity>(newAppUserIdentity
                , AppUserIdentityTableName
                , newAppUserIdentity.AppUserIdentityId
                , customerId
                , appUserId);
        return Results.Created($"{BaseService}/{newAppUserIdentity.AppUserIdentityId}", newAppUserIdentity);
    }

    internal static async Task<IResult> UpdateAppUserIdentityAsync(UpdateAppUserIdentityRequest request
            , IValidator<UpdateAppUserIdentityRequest> requestValidator
            , IMasterRepository masterRepository
            , IAppUserIdentityRepository appUserIdentityRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        AppUserIdentity? existingRecord = await appUserIdentityRepository.GetAppUserIdentityByIdAsync(request.AppUserIdentityId);
        if (existingRecord == null)
            return Results.BadRequest(new
            {
                Code = "APP_USER_IDENTITY_RECORD_NOT_FOUND",
                Note = $"AppUser email with id ({request.AppUserIdentityId}) does not exist."
            });
        var existingRecordCopy = new AppUserIdentity(existingRecord);

        byte identityTypeId = await masterRepository.GetIdentityTypeIdAsync(request.IdentityTypeCode);
        if (identityTypeId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_IDENTITY_TYPE_ID",
                Note = $"Identity Type ({request.IdentityTypeCode}) is unrecognized."
            });

        var matchingOtherIdentityRecord = await appUserIdentityRepository.GetAppUserIdentityByIdentityTypeAndNumberAsync(existingRecord.AppUserId
            , identityTypeId, request.IdentityNumber);
        if (matchingOtherIdentityRecord != null
            && matchingOtherIdentityRecord.AppUserIdentityId != existingRecord.AppUserIdentityId)
            return Results.Conflict<AppUserIdentity>(existingRecord);

        AppUserIdentity updatedAppUserIdentity = await appUserIdentityRepository.UpdateAppUserIdentityAsync(request, identityTypeId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordEditModelAuditDataAsync<AppUserIdentity>(existingRecordCopy
                , updatedAppUserIdentity
                , AppUserIdentityTableName
                , updatedAppUserIdentity.AppUserIdentityId
                , customerId
                , appUserId);

        return Results.Accepted($"{BaseService}/{updatedAppUserIdentity.AppUserIdentityId}", updatedAppUserIdentity);
    }

    internal static async Task<IResult> DeleteAppUserIdentityAsync(long id
            , IAppUserIdentityRepository appUserIdentityRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        await appUserIdentityRepository.DeactivateAppUserIdentityAsync(id);
        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordDeactivatedAuditDataAsync(AppUserIdentityTableName
                , id
                , customerId
                , appUserId);
        return Results.NoContent();
    }

    internal static async Task<IResult> GetAppUserIdentityByIdAsync(long id
        , IAppUserIdentityRepository appUserIdentityRepository)
    {
        AppUserIdentityModel? appUserIdentity = await appUserIdentityRepository.GetAppUserIdentityModelByIdAsync(id);
        return Results.Ok(appUserIdentity);
    }

    internal static async Task<IResult> GetAppUserIdentitiesByAppUserIdAsync(long id
            , IAppUserIdentityRepository appUserIdentityRepository)
    {
        IList<AppUserIdentityModel> identities = await appUserIdentityRepository.GetAppUserIdentityModelsByAppUserIdAsync(id);
        return Results.Ok(identities);
    }

    public static void Register(WebApplication app)
    {
        app.MapPost($"{BaseService}", InsertAppUserIdentityAsync)
        .WithName("InsertAppUserIdentity")
        .WithDescription("Inserts a new app user identity record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<AppUserIdentity>(409)
        .Produces<AppUserIdentity>(201)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPut($"{BaseService}", UpdateAppUserIdentityAsync)
        .WithName("UpdateAppUserIdentity")
        .WithDescription("Updates a app user identity record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<AppUserIdentity>(409)
        .Produces<AppUserIdentity>(202)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapDelete(string.Join("/", BaseService, "{id}"), DeleteAppUserIdentityAsync)
        .WithName("DeactivateAppUserIdentity")
        .WithDescription("Deactivate a app user identity record.")
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<AppUserAddress>(204)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapGet(string.Join("/", BaseService, "{id}"), GetAppUserIdentityByIdAsync)
        .WithName("GetAppUserIdentityById")
        .WithDescription("Fetches app user identity record using appUser identity id.")
        .Produces<AppUserIdentityModel>(200)
        .Produces(401)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapGet(string.Join("/", BaseService, "appuser/{id}"), GetAppUserIdentitiesByAppUserIdAsync)
        .WithName("GetAppUserIdentitiesByAppUserId")
        .WithDescription("Fetches app user identity records using appUser id.")
        .Produces<AppUserIdentityModel[]>(200)
        .Produces(401)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });
    }
}
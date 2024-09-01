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

public class AppUserPhoneService
{
    private const string BaseService = "/api/users/phones";
    private const string AppUserPhoneTableName = "appuserphone";

    internal static async Task<IResult> GetAppUserPhoneModelByIdAsync(long id
            , IAppUserPhoneRepository appUserPhoneRepository)
    {
        AppUserPhoneModel? appUserPhoneModel = await appUserPhoneRepository.GetAppUserPhoneModelByIdAsync(id);
        return Results.Ok(appUserPhoneModel);
    }

    internal static async Task<IResult> GetAppUserPhoneModelsByAppUserIdAsync(long id
            , IAppUserPhoneRepository appUserPhoneRepository)
    {
        IList<AppUserPhoneModel> appUserPhoneModels = await appUserPhoneRepository.GetAppUserPhoneModelsByAppUserIdAsync(id);
        return Results.Ok(appUserPhoneModels);
    }

    internal static async Task<IResult> InsertAppUserPhoneAsync(InsertAppUserPhoneRequest request
            , IValidator<InsertAppUserPhoneRequest> requestValidator
            , IMasterRepository masterRepository
            , IAppUserPhoneRepository appUserPhoneRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var existingRecord = await appUserPhoneRepository.GetAppUserPhoneByPhoneNumberAsync(
                                request.AppUserId
                                , request.PhoneNumber);
        if (existingRecord != null)
            return Results.Conflict<AppUserPhone>(existingRecord);

        byte phoneTypeId = await masterRepository.GetPhoneTypeIdAsync(request.PhoneTypeCode);
        if (phoneTypeId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_PHONE_TYPE_CODE",
                Note = $"Phone type code ({request.PhoneTypeCode}) is unrecognized."
            });

        AppUserPhone newAppUserPhone = await appUserPhoneRepository.InsertAppUserPhoneAsync(request
            , phoneTypeId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordNewModelAuditDataAsync<AppUserPhone>(newAppUserPhone
                , AppUserPhoneTableName
                , newAppUserPhone.AppUserPhoneId
                , customerId
                , appUserId);

        return Results.Created($"{BaseService}/{newAppUserPhone.AppUserPhoneId}", newAppUserPhone);
    }

    internal static async Task<IResult> UpdateAppUserPhoneAsync(UpdateAppUserPhoneRequest request
            , IValidator<UpdateAppUserPhoneRequest> requestValidator
            , IMasterRepository masterRepository
            , IAppUserPhoneRepository appUserPhoneRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var existingRecord = await appUserPhoneRepository.GetAppUserPhoneByIdAsync(request.AppUserPhoneId);
        if (existingRecord == null)
            return Results.BadRequest(new
            {
                Code = "APP_USER_PHONE_RECORD_NOT_FOUND",
                Note = $"App User phone with id ({request.AppUserPhoneId}) does not exist."
            });
        var existingRecordCopy = new AppUserPhone(existingRecord);

        var matchingOtherAppUserPhone = await appUserPhoneRepository.GetAppUserPhoneByPhoneNumberAsync(existingRecord.AppUserId
            , request.PhoneNumber);
        if (matchingOtherAppUserPhone != null
            && matchingOtherAppUserPhone.AppUserPhoneId != request.AppUserPhoneId)
            return Results.Conflict<AppUserPhone>(existingRecord);

        byte phoneTypeId = await masterRepository.GetPhoneTypeIdAsync(request.PhoneTypeCode);
        if (phoneTypeId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_PHONE_TYPE_CODE",
                Note = $"Phone type code ({request.PhoneTypeCode}) is unrecognized."
            });

        AppUserPhone updatedAppUserPhone = await appUserPhoneRepository.UpdateAppUserPhoneAsync(request, phoneTypeId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordEditModelAuditDataAsync<AppUserPhone>(existingRecordCopy
                , updatedAppUserPhone
                , AppUserPhoneTableName
                , updatedAppUserPhone.AppUserPhoneId
                , customerId
                , appUserId);

        return Results.Accepted($"{BaseService}/{updatedAppUserPhone.AppUserPhoneId}", updatedAppUserPhone);
    }

    internal static async Task<IResult> DeleteAppUserPhoneAsync(long id
            , IAppUserPhoneRepository appUserPhoneRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        await appUserPhoneRepository.DeactivateAppUserPhoneAsync(id);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordDeactivatedAuditDataAsync(AppUserPhoneTableName
                , id
                , customerId
                , appUserId);
        return Results.NoContent();
    }

    public static void Register(WebApplication app)
    {
        app.MapGet(string.Join("/", BaseService, "{id}"), GetAppUserPhoneModelByIdAsync)
        .WithName("GetAppUserPhoneById")
        .WithDescription("Fetches app user phone record using app user phone id.")
        .Produces<AppUserPhoneModel>(200)
        .Produces(401)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapGet(string.Join("/", BaseService, "appuser/{id}"), GetAppUserPhoneModelsByAppUserIdAsync)
        .WithName("GetAppUserPhoneByAppUserId")
        .WithDescription("Fetches app user phone records using app user id.")
        .Produces<AppUserPhoneModel[]>(200)
        .Produces(401)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPost($"{BaseService}", InsertAppUserPhoneAsync)
        .WithName("InsertAppUserPhone")
        .WithDescription("Inserts a new app user phone record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces(401)
        .Produces<AppUserPhone>(409)
        .Produces<AppUserPhone>(201)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPut($"{BaseService}", UpdateAppUserPhoneAsync)
        .WithName("UpdateAppUserPhone")
        .WithDescription("Inserts a new app user phone record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces(401)
        .Produces<AppUserPhone>(409)
        .Produces<AppUserPhone>(202)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapDelete(string.Join("/", BaseService, "{id}"), DeleteAppUserPhoneAsync)
        .WithName("DeactivateAppUserPhone")
        .WithDescription("Deactivate a app user phone record.")
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces(204)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });
    }
}
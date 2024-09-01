using System.Security.Claims;
using FluentValidation;
using Health.Api.Authentication;
using Health.Api.Data;
using Health.Api.Models.Constants;
using Health.Api.Models.Requests.User;
using Health.Api.Repositories;
using Health.Api.Repositories.Users;

namespace Health.Api.Services.Users;

public class AppUserEmailService
{
    private const string BaseService = "/api/users/emails";
    private const string AppUserEmailTableName = "appUseremail";

    internal static async Task<IResult> GetAppUserEmailByIdAsync(long id
            , IAppUserEmailRepository appUserEmailRepository)
    {
        AppUserEmail? appUserEmail = await appUserEmailRepository.GetAppUserEmailByIdAsync(id);
        return Results.Ok(appUserEmail);
    }

    internal static async Task<IResult> GetAppUserEmailsByAppUserIdAsync(long id
            , IAppUserEmailRepository appUserEmailRepository)
    {
        IList<AppUserEmail> appUserEmails = await appUserEmailRepository.GetAppUserEmailsByAppUserIdAsync(id);
        return Results.Ok(appUserEmails);
    }

    internal static async Task<IResult> InsertAppUserEmailAsync(InsertAppUserEmailRequest request
            , IValidator<InsertAppUserEmailRequest> requestValidator
            , IAppUserEmailRepository appUserEmailRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var existingRecord = await appUserEmailRepository.GetAppUserEmailByEmailAddressAsync(request.AppUserId, request.EmailAddress);
        if (existingRecord != null)
            return Results.Conflict<AppUserEmail>(existingRecord);

        AppUserEmail newAppUserEmail = await appUserEmailRepository.InsertAppUserEmailAsync(request);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordNewModelAuditDataAsync<AppUserEmail>(newAppUserEmail
                , AppUserEmailTableName
                , newAppUserEmail.AppUserEmailId
                , customerId
                , appUserId);

        return Results.Created($"{BaseService}/{newAppUserEmail.AppUserEmailId}", newAppUserEmail);
    }

    internal static async Task<IResult> UpdateAppUserEmailAsync(UpdateAppUserEmailRequest request
            , IValidator<UpdateAppUserEmailRequest> requestValidator
            , IAppUserEmailRepository appUserEmailRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var existingRecord = await appUserEmailRepository.GetAppUserEmailByIdAsync(request.AppUserEmailId);
        if (existingRecord == null)
            return Results.BadRequest(new
            {
                Code = "APP_USER_EMAIL_RECORD_NOT_FOUND",
                Note = $"AppUser email with id ({request.AppUserEmailId}) does not exist."
            });
        var existingRecordCopy = new AppUserEmail(existingRecord);

        var matchingOtherEmailRecord = await appUserEmailRepository.GetAppUserEmailByEmailAddressAsync(existingRecord.AppUserId
            , request.EmailAddress);
        if (matchingOtherEmailRecord != null && matchingOtherEmailRecord.AppUserEmailId != request.AppUserEmailId)
            return Results.Conflict<AppUserEmail>(matchingOtherEmailRecord);

        AppUserEmail updatedAppUserEmail = await appUserEmailRepository.UpdateAppUserEmailAsync(request);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordEditModelAuditDataAsync<AppUserEmail>(existingRecordCopy
                , updatedAppUserEmail
                , AppUserEmailTableName
                , updatedAppUserEmail.AppUserEmailId
                , customerId
                , appUserId);

        return Results.Accepted($"{BaseService}/{updatedAppUserEmail.AppUserEmailId}", updatedAppUserEmail);
    }

    internal static async Task<IResult> DeactivateAppUserEmailAsync(long id
            , IAppUserEmailRepository appUserEmailRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        await appUserEmailRepository.DeactivateAppUserEmailAsync(id);
        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordDeactivatedAuditDataAsync(AppUserEmailTableName
                , id
                , customerId
                , appUserId);
        return Results.NoContent();
    }

    public static void Register(WebApplication app)
    {
        app.MapGet(string.Join("/", BaseService, "{id}"), GetAppUserEmailByIdAsync)
        .WithName("GetAppUserEmailById")
        .WithDescription("Fetches app user email record using app user email id.")
        .Produces<AppUserEmail>(200)
        .Produces(401)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapGet(string.Join("/", BaseService, "appuser/{id}"), GetAppUserEmailsByAppUserIdAsync)
        .WithName("GetAppUserEmailsByAppUserId")
        .WithDescription("Fetches appUser email records using app user id.")
        .Produces<AppUserEmail[]>(200)
        .Produces(401)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPost($"{BaseService}", InsertAppUserEmailAsync)
        .WithName("InsertAppUserEmail")
        .WithDescription("Inserts a new app user email record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces(401)
        .Produces<AppUserEmail>(409)
        .Produces<AppUserEmail>(201)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPut($"{BaseService}", UpdateAppUserEmailAsync)
        .WithName("UpdateAppUserEmail")
        .WithDescription("Inserts a new app user email record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces(401)
        .Produces<AppUserEmail>(409)
        .Produces<AppUserEmail>(201)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapDelete(string.Join("/", BaseService, "{id}"), DeactivateAppUserEmailAsync)
        .WithName("DeactivateAppUserEmail")
        .WithDescription("Deactivate a app user email record.")
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<AppUserAddress>(204)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });
    }
}
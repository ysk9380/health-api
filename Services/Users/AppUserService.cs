using System.Security.Claims;
using FluentValidation;
using Health.Api.Authentication;
using Health.Api.Data;
using Health.Api.Models.Constants;
using Health.Api.Models.Requests;
using Health.Api.Models.Requests.User;
using Health.Api.Models.Responses;
using Health.Api.Models.User;
using Health.Api.Repositories;
using Health.Api.Repositories.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;

namespace Health.Api.Services.Users;

public class AppUserService
{
    private const string BaseService = "/api/users";
    private const string RefreshTokenKey = "refresh_token";
    private const string AppUserTableName = "appuser";
    private const string CustomerAppUserTableName = "customerappuser";

    internal static async Task<IResult> LoginAsync(LoginRequest request
        , IValidator<LoginRequest> loginRequestValidator
        , IAppUserRepository appUserRepository
        , ITokenManager tokenManager
        , ILogger<AppUserService> logger
        , HttpResponse response)
    {
        var validationResult = loginRequestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        logger.LogInformation("Login initiated for {customerCode}, {username}", request.CustomerCode, request.Username);

        LoginResponse? loginResponse = await appUserRepository.LoginAsync(request);
        if (loginResponse != null)
        {
            logger.LogInformation("Login successful for {customerCode}, {username}", request.CustomerCode, request.Username);

            string accessToken = tokenManager.GetAccessToken(loginResponse);
            var (refreshTokenId, refreshToken) = tokenManager.GetRefreshToken();
            await appUserRepository.UpdateTokenAsync(loginResponse.AppUserId, loginResponse.CustomerId, refreshTokenId.ToString());
            response.Cookies.Append(RefreshTokenKey, refreshToken, new CookieOptions
            {
                Expires = DateTime.Now.AddDays(1),
                HttpOnly = true,
                IsEssential = true,
                MaxAge = new TimeSpan(1, 0, 0, 0),
                Secure = false,
            });

            return Results.Ok(accessToken);
        }

        logger.LogInformation("Login failed for {customerCode}, {username}", request.CustomerCode, request.Username);
        return Results.Unauthorized();
    }

    internal static async Task<IResult> AdminLoginAsync(AdminLoginRequest adminLoginRequest
        , IValidator<AdminLoginRequest> loginRequestValidator
        , IAppUserRepository appUserRepository
        , ITokenManager tokenManager
        , IConfiguration configuration
        , ILogger<AppUserService> logger
        , HttpResponse response)
    {
        var validationResult = loginRequestValidator.Validate(adminLoginRequest);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        string adminCustomerCode = configuration.GetValue<string>("AdminCustomerCode")!;
        var request = new LoginRequest
        {
            CustomerCode = adminCustomerCode,
            Username = adminLoginRequest.Username,
            Password = adminLoginRequest.Password
        };

        logger.LogInformation("Admin Login initiated for {customerCode}, {username}", request.CustomerCode, request.Username);

        LoginResponse? loginResponse = await appUserRepository.LoginAsync(request);
        if (loginResponse != null)
        {
            if (!loginResponse.RoleCode.Equals("SYSADMIN"))
                return Results.Forbid();

            logger.LogInformation("Admin Login successful for {customerCode}, {username}", request.CustomerCode, request.Username);

            string accessToken = tokenManager.GetAccessToken(loginResponse);
            var (refreshTokenId, refreshToken) = tokenManager.GetRefreshToken();
            await appUserRepository.UpdateTokenAsync(loginResponse.AppUserId, loginResponse.CustomerId, refreshTokenId.ToString());
            response.Cookies.Append(RefreshTokenKey, refreshToken, new CookieOptions
            {
                Expires = DateTime.Now.AddDays(1),
                HttpOnly = true,
                IsEssential = true,
                MaxAge = new TimeSpan(1, 0, 0, 0),
                Secure = false,
            });

            return Results.Ok(accessToken);
        }

        logger.LogInformation("Admin Login failed for {customerCode}, {username}", request.CustomerCode, request.Username);
        return Results.Unauthorized();
    }

    internal static async Task<IResult> ChangePasswordAsync(ChangePasswordRequest request
            , ClaimsPrincipal userPrincipal
            , IAppUserRepository appUserRepository)
    {
        string? customerCodeFromToken = userPrincipal.Claims
                                        .Where(c => c.Type.Equals(ClaimType.CustomerCode))
                                        .Select(s => s.Value)
                                        .FirstOrDefault();

        if (!string.Equals(request.CustomerCode, customerCodeFromToken))
            return Results.Forbid();

        var customerAppUser = await appUserRepository.GetCustomerAppUserAsync(request.CustomerCode
            , request.Username
            , request.OldPassword);

        if (customerAppUser == null)
        {
            return Results.Ok(new ChangePasswordResponse
            {
                IsSuccess = false,
                Result = "OLD_PASSWORD_MISMATCH"
            });
        }

        bool result = await appUserRepository
            .UpdateCustomerAppUserPasswordAsync(customerAppUser.CustomerAppUserId, request.NewPassword);
        return Results.Ok(new ChangePasswordResponse
        {
            IsSuccess = result
        });
    }

    internal static async Task<IResult> ChangeAppUserPasswordAsync(ChangeAppUserPasswordRequest request
            , IValidator<ChangeAppUserPasswordRequest> requestValidator
            , ClaimsPrincipal userPrincipal
            , IAppUserRepository appUserRepository)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        string? customerCodeFromToken = userPrincipal.Claims
                                        .Where(c => c.Type.Equals(ClaimType.CustomerCode))
                                        .Select(s => s.Value)
                                        .FirstOrDefault();

        var customerAppUser = await appUserRepository.GetCustomerAppUserAsync(request.CustomerAppUserId);
        if (customerAppUser == null)
        {
            return Results.BadRequest(new
            {
                Code = "CUSTOMER_APP_USER_ID_NOT_FOUND",
                Note = $"Customer App User Id ({request.CustomerAppUserId}) does not exist.",
            });
        }

        bool result = await appUserRepository
            .UpdateCustomerAppUserPasswordAsync(customerAppUser.CustomerAppUserId, request.NewPassword);
        return Results.Ok(new ChangePasswordResponse
        {
            IsSuccess = result
        });
    }

    internal static async Task<IResult> RefreshTokenAsync(HttpRequest request
        , HttpResponse response
        , ITokenManager tokenManager
        , IAppUserRepository appUserRepository
        , ILogger<AppUserService> logger)
    {
        var refreshToken = request.Cookies["refresh_token"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            logger.LogDebug("MISSING_REFRESH_TOKEN");
            return Results.Forbid();
        }

        if (!tokenManager.TryValidateRefreshToken(refreshToken, out Guid refreshTokenId))
        {
            logger.LogDebug("INVALID_REFRESH_TOKEN");
            return Results.Forbid();
        }

        LoginResponse? loginResponse = await appUserRepository.GetLoginResponseUsingRefreshTokenIdAsync(refreshTokenId.ToString());
        if (loginResponse == null)
        {
            logger.LogDebug("Login with refresh token failed. {refreshTokenId}", refreshTokenId);
            return Results.Forbid();
        }

        string accessToken = tokenManager.GetAccessToken(loginResponse);
        var (newRefreshTokenId, newRefreshToken) = tokenManager.GetRefreshToken();
        await appUserRepository.UpdateTokenAsync(loginResponse.AppUserId, loginResponse.CustomerId, newRefreshTokenId.ToString());
        response.Cookies.Append(RefreshTokenKey, newRefreshToken, new CookieOptions
        {
            Expires = DateTime.Now.AddDays(1),
            HttpOnly = true,
            IsEssential = true,
            MaxAge = new TimeSpan(1, 0, 0, 0),
            Secure = true,
            SameSite = SameSiteMode.Strict
        });

        return Results.Ok(accessToken);
    }

    internal static async Task<IResult> LogoutAsync(ClaimsPrincipal userPrincipal
        , IAppUserRepository appUserRepository
        , HttpResponse response)
    {

        long userId = Convert.ToInt64(userPrincipal.Claims
                                .Where(c => c.Type.Equals(ClaimType.AppUserId))
                                .Select(s => s.Value).First());
        long customerId = Convert.ToInt64(userPrincipal.Claims
                            .Where(c => c.Type.Equals(ClaimType.CustomerId))
                            .Select(s => s.Value).First());

        await appUserRepository.UpdateTokenAsync(customerId, userId, null);
        response.Cookies.Delete(RefreshTokenKey);
        return Results.NoContent();
    }

    internal static async Task<IResult> ChangeLanguageAsync([FromBody] string languageCode
        , ClaimsPrincipal userPrincipal
        , IMasterRepository masterRepository
        , IAppUserRepository appUserRepository)
    {
        byte languageId = await masterRepository.GetLanguageIdAsync(languageCode);
        if (languageId == default)
            return Results.BadRequest(new { Code = "INVALID_LANGUAGE_CODE", Note = "Language code is invalid." });

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        bool result = await appUserRepository.UpdateCustomerAppUserLanguageAsync(customerId, appUserId, languageId);
        return Results.Ok(result);
    }

    internal static async Task<IResult> InsertAppUserAsync(InsertAppUserRequest request
        , IValidator<InsertAppUserRequest> requestValidator
        , IAppUserRepository appUserRepository
        , IMasterRepository masterRepository
        , IDataAuditHistoryRepository dataAuditHistoryRepository
        , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        if (!request.ForceCreate)
        {
            var existingRecord
                = await appUserRepository.GetAppUserByNameAndDoBAsync(request.Firstname
                    , request.Lastname
                    , request.DateOfBirth);

            if (existingRecord != null)
                return Results.Conflict<AppUser>(existingRecord);
        }

        byte genderId = await masterRepository.GetGenderIdAsync(request.GenderCode);
        if (genderId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_GENDER_CODE",
                Note = $"Gender code ({request.GenderCode}) is unrecognized."
            });

        AppUser newAppUser = await appUserRepository.InsertNewAppUserAsync(request, genderId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordNewModelAuditDataAsync<AppUser>(newAppUser
                , AppUserTableName
                , newAppUser.AppUserId
                , customerId
                , appUserId);

        return Results.Created($"{BaseService}/{newAppUser.AppUserId}", newAppUser);
    }

    internal static async Task<IResult> CreateLoginAsync(CreateLoginRequest request
        , IValidator<CreateLoginRequest> requestValidator
        , IMasterRepository masterRepository
        , IAppUserRepository appUserRepository
        , IDataAuditHistoryRepository dataAuditHistoryRepository
        , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);

        byte languageId = 0;
        IList<Language> languages = await masterRepository.GetLanguagesAsync();
        if (languages.Any(l => l.LanguageCode.Equals(request.LanguageCode)))
            languageId = languages.Where(l => l.LanguageCode.Equals(request.LanguageCode))
                            .Select(s => s.LanguageId)
                            .First();
        else
            languageId = languages.Where(l => l.LanguageCode.Equals("en"))
                            .Select(s => s.LanguageId)
                            .First();

        byte appUserRoleId = await masterRepository.GetAppUserRoleIdAsync(request.RoleCode);
        if (appUserRoleId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_ROLE_CODE",
                Note = $"Role code ({request.RoleCode}) is unrecognized."
            });

        CustomerAppUser? existingRecord
            = await appUserRepository.GetCustomerAppUserAsync(request.CustomerId, request.AppUserId);
        if (existingRecord != null)
        {
            CustomerAppUser existingCustomerAppUserCopy = new CustomerAppUser(existingRecord);

            CustomerAppUser updatedCustomerAppUser
                = await appUserRepository.ActivateCustomerAppUserAsync(request, existingRecord.AppUserId, appUserRoleId);

            await dataAuditHistoryRepository.RecordEditModelAuditDataAsync<CustomerAppUser>(existingCustomerAppUserCopy
            , updatedCustomerAppUser
            , CustomerAppUserTableName
            , updatedCustomerAppUser.CustomerAppUserId
            , customerId
            , appUserId);

            return Results.Accepted("/api/users/login", updatedCustomerAppUser != null);
        }
        else
        {
            CustomerAppUser? existingCustomerAppUserWithSameUsername
                = await appUserRepository.GetCustomerAppUserByUsernameAsync(request.CustomerId, request.EmailAddress);

            if (existingCustomerAppUserWithSameUsername != null)
                return Results.Conflict<long>(existingCustomerAppUserWithSameUsername.CustomerAppUserId);

            CustomerAppUser newCustomerAppUser
                = await appUserRepository.InsertCustomerAppUserAsync(request, languageId, appUserRoleId);

            await dataAuditHistoryRepository.RecordNewModelAuditDataAsync<CustomerAppUser>(newCustomerAppUser
                , CustomerAppUserTableName
                , newCustomerAppUser.CustomerAppUserId
                , customerId
                , appUserId);
            return Results.Created("/api/users/login", newCustomerAppUser.CustomerAppUserId > 0);
        }
    }

    internal static async Task<IResult> RemoveLoginAsync(long id
            , IAppUserRepository appUserRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        bool result
            = await appUserRepository.DeactivateCustomerAppUserAsync(id);

        if (result)
        {
            await dataAuditHistoryRepository.RecordDeactivatedAuditDataAsync(CustomerAppUserTableName
                , id, customerId, appUserId);
            return Results.NoContent();
        }
        else
        {
            return Results.BadRequest(new
            {
                Code = "CUSTOMER_APP_USER_ID_NOT_FOUND",
                Note = $"Customer App User Id ({id}) does not exist.",
            });
        }
    }

    internal static async Task<IResult> SearchAppUsersAsync(AppUserSearchRequest request
            , IAppUserRepository appUserRepository)
    {
        if (string.IsNullOrWhiteSpace(request.Firstname)
            && string.IsNullOrWhiteSpace(request.Lastname)
            && string.IsNullOrWhiteSpace(request.PhoneNumber)
            && string.IsNullOrWhiteSpace(request.Email)
            && string.IsNullOrWhiteSpace(request.IdentityNumber))
            return Results.NoContent();

        IList<AppUserSearchModel> searchResult = await appUserRepository.SearchAppUsersAsync(request);

        return Results.Ok(searchResult);
    }

    internal static async Task<IResult> GetAppUserAsync(long appUserId, IAppUserRepository appUserRepository)
    {
        AppUserModel? appUserModel = await appUserRepository.GetAppUserModelAsync(appUserId);
        return Results.Ok(appUserModel);
    }

    internal static async Task<IResult> UpdateAppUserAsync(UpdateAppUserRequest request
        , IValidator<UpdateAppUserRequest> requestValidator
        , IAppUserRepository appUserRepository
        , IMasterRepository masterRepository
        , IDataAuditHistoryRepository dataAuditHistoryRepository
        , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var existingOtherRecord
            = await appUserRepository.GetOtherAppUserWithSameNameAndDoBAsync(request.Firstname
                , request.Lastname
                , request.DateOfBirth
                , request.AppUserId);

        if (existingOtherRecord != null)
            return Results.Conflict<AppUser>(existingOtherRecord);

        byte genderId = await masterRepository.GetGenderIdAsync(request.GenderCode);
        if (genderId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_GENDER_CODE",
                Note = $"Gender code ({request.GenderCode}) is unrecognized."
            });

        var existingRecord = await appUserRepository.GetAppUserAsync(request.AppUserId);
        if (existingRecord == null)
            return Results.BadRequest(new
            {
                Code = "APPUSER_RECORD_NOT_FOUND",
                Note = $"AppUser with id ({request.AppUserId}) does not exist."
            });
        var existingRecordCopy = new AppUser(existingRecord);

        AppUser updatedRecord = await appUserRepository.UpdateAppUserAsync(request, genderId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordEditModelAuditDataAsync<AppUser>(existingRecordCopy
                , updatedRecord
                , AppUserTableName
                , updatedRecord.AppUserId
                , customerId
                , appUserId);

        return Results.Accepted($"{BaseService}/{updatedRecord.AppUserId}", updatedRecord);
    }

    internal static async Task<IResult> GetCustomerAppUsersAsync(long customerId, IAppUserRepository appUserRepository)
    {
        IList<CustomerAppUserModel> customerAppUserModels = await appUserRepository.GetCustomerAppUsersAsync(customerId);
        return Results.Ok(customerAppUserModels);
    }

    internal static async Task<IResult> GetCustomerAppUserAsync(CustomerAppUserRequest request, IAppUserRepository appUserRepository)
    {
        CustomerAppUser? customerAppUser = await appUserRepository.GetCustomerAppUserAsync(request.CustomerId, request.AppUserId);
        if (customerAppUser != null)
            return Results.Ok(customerAppUser);
        else
            return Results.NoContent();
    }

    internal static async Task<IResult> ActivateLoginAsync(long id
            , IAppUserRepository appUserRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        bool result
            = await appUserRepository.ActivateCustomerAppUserAsync(id);

        if (result)
        {
            await dataAuditHistoryRepository.RecordActivatedAuditDataAsync(CustomerAppUserTableName
                , id, customerId, appUserId);
            return Results.Accepted();
        }
        else
        {
            return Results.BadRequest(new
            {
                Code = "CUSTOMER_APP_USER_ID_NOT_FOUND",
                Note = $"Customer App User Id ({id}) does not exist.",
            });
        }
    }

    public static void Register(WebApplication app)
    {
        app.MapPost($"{BaseService}/login", LoginAsync)
        .WithName("Login")
        .WithDescription("API enables the user to authenticate using customerCode, username and password.")
        .Produces<string>(200)
        .Produces<HttpValidationProblemDetails>(400)
        .Produces(401)
        .AllowAnonymous();

        app.MapPost($"{BaseService}/admin-login", AdminLoginAsync)
        .WithName("AdminLogin")
        .WithDescription("API enables the user to authenticate using username and password and allows only users with role as sys admin to login.")
        .Produces<string>(200)
        .Produces<HttpValidationProblemDetails>(400)
        .Produces(401)
        .Produces(403)
        .AllowAnonymous();

        app.MapPatch($"{BaseService}/changepassword", ChangePasswordAsync)
        .WithName("ChangePassword")
        .WithDescription("Verify the old password and update the new password on successful verification.")
        .Produces(403)
        .Produces<ChangePasswordResponse>(200)
        .RequireAuthorization();

        app.MapPatch($"{BaseService}/changeappuserpassword", ChangeAppUserPasswordAsync)
        .WithName("ChangeAppUserPassword")
        .WithDescription("Reset password for customer's app user.")
        .Produces(400)
        .Produces<ChangePasswordResponse>(200)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPatch($"{BaseService}/refreshtoken", RefreshTokenAsync)
        .WithName("Refresh access token")
        .WithDescription("Using the refresh token, generate a new access token.")
        .Produces<LoginResponse>(200)
        .Produces(403)
        .AllowAnonymous();

        app.MapPost($"{BaseService}/logout", LogoutAsync)
        .WithName("Logout")
        .WithDescription("Logout the user and invalidate the refresh token.")
        .Produces<LoginResponse>(200)
        .Produces<HttpValidationProblemDetails>(400)
        .Produces(401)
        .RequireAuthorization();

        app.MapPatch($"{BaseService}/changelanguage", ChangeLanguageAsync)
        .WithName("ChangeLanguage")
        .WithDescription("Change the language in which the UI application will be presented to the user.")
        .Produces<bool>(200)
        .Produces<dynamic>(400)
        .Produces(401)
        .RequireAuthorization();

        app.MapPost(BaseService, InsertAppUserAsync)
        .WithName("InsertAppUser")
        .WithDescription("Inserts a new app user record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<AppUser>(409)
        .Produces<AppUser>(201)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPost($"{BaseService}/login/create", CreateLoginAsync)
        .WithName("CreateCustomerAppUserLogin")
        .WithDescription("Creates a new app user login for specified customer user.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces<long>(409)
        .Produces(401)
        .Produces<bool>(201) // when a new login is created
        .Produces<bool>(202) // when existing login is updated and reactivated
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapDelete(string.Join("", BaseService, "/login/{id}"), RemoveLoginAsync)
        .WithName("RemoveCustomerAppUserLogin")
        .WithDescription("Removes app user login for specified customer user.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400) // when a record with id is not found.
        .Produces<bool>(204)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPost(string.Join("/", BaseService, "search"), SearchAppUsersAsync)
        .WithName("SearchAppUsers")
        .WithDescription("Searches app user records based on search criteria provided.")
        .Produces<AppUserSearchModel[]>(200)
        .Produces(401)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapGet(string.Join("/", BaseService, "{appUserId}"), GetAppUserAsync)
        .WithName("GetAppUser")
        .WithDescription("Get app user information.")
        .Produces<AppUserModel[]>(200)
        .Produces(204)
        .Produces(401)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPut(BaseService, UpdateAppUserAsync)
        .WithName("UpdateAppUser")
        .WithDescription("Update an existing app user record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<AppUser>(409)
        .Produces<AppUser>(202)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapGet(string.Join("/", BaseService, "customer/{customerId}"), GetCustomerAppUsersAsync)
        .WithName("GetCustomerAppUsers")
        .WithDescription("Get app users information for the given customer.")
        .Produces<CustomerAppUserModel[]>(200)
        .Produces(204)
        .Produces(401)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPatch(string.Join("", BaseService, "/login/reactivate/{id}"), ActivateLoginAsync)
        .WithName("ActivateCustomerAppUserLogin")
        .WithDescription("Activate login for specified customer user.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces<bool>(202)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPost(string.Join("/", BaseService, "customerappuser"), GetCustomerAppUserAsync)
        .WithName("GetCustomerAppUser")
        .WithDescription("Get app users information for the given customer.")
        .Produces<CustomerAppUser>(200)
        .Produces<CustomerAppUser>(204)
        .Produces(204)
        .Produces(401)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        AppUserAddressService.Register(app);
        AppUserPhoneService.Register(app);
        AppUserEmailService.Register(app);
        AppUserIdentityService.Register(app);
    }
}
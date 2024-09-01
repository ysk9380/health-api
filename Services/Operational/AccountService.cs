using FluentValidation;
using Health.Api.Data;
using Health.Api.Models.Constants;
using Health.Api.Models.Requests.Operational;
using Health.Api.Repositories.Operational;

namespace Health.Api.Services.Operational;

public class AccountService
{
    private const string BaseService = "/api/operational/accounts";

    internal static async Task<IResult> GetAccountsAsync(IAccountRepository accountRepository)
    {
        IList<Account> accounts = await accountRepository.GetAccountsAsync();
        return Results.Ok(accounts);
    }

    internal static async Task<IResult> GetAccountByIdAsync(byte id
        , IAccountRepository accountRepository)
    {
        Account? account = await accountRepository.GetAccountByIdAsync(id);
        return Results.Ok(account);
    }

    internal static async Task<IResult> GetAccountByCodeAsync(string code
        , IAccountRepository accountRepository)
    {
        Account? account = await accountRepository.GetAccountByCodeAsync(code);
        return Results.Ok(account);
    }

    internal static async Task<IResult> InsertAccountAsync(InsertAccountRequest request
        , IValidator<InsertAccountRequest> requestValidator
        , IAccountRepository accountRepository
        , ILogger<AccountService> logger)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        Account? existingRecord = await accountRepository.GetAccountByCodeAsync(request.AccountCode);
        if (existingRecord != null)
        {
            logger.LogError("Account Insertion Conflict. Another account existing with same code {accountCode}"
                , request.AccountCode);
            return Results.Conflict<Account>(existingRecord);
        }

        Account newAccount = await accountRepository.InsertAccountAsync(request);
        return Results.Created($"api/operational/accounts/{newAccount.AccountId}", newAccount);
    }

    internal static async Task<IResult> UpdateAccountAsync(UpdateAccountRequest request
        , IValidator<UpdateAccountRequest> requestValidator
        , IAccountRepository accountRepository
        , ILogger<AccountService> logger)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        Account? existingRecord = await accountRepository.GetAccountByCodeAsync(request.AccountCode);
        if (existingRecord != null && existingRecord.AccountId != request.AccountId)
        {
            logger.LogError("Deparment Update Conflict. Another account existing with same code {accountCode}"
                , request.AccountCode);
            return Results.Conflict<Account>(existingRecord);
        }

        Account updatedAccount = await accountRepository.UpdateAccountAsync(request);
        return Results.Created($"api/operational/accounts/{updatedAccount.AccountId}", updatedAccount);
    }

    public static void Register(WebApplication app)
    {
        app.MapGet(BaseService, GetAccountsAsync)
        .WithName("GetAccounts")
        .WithDescription("Returns the list of accounts.")
        .Produces<IList<Account>>(200)
        .RequireAuthorization();

        app.MapGet(string.Join("", BaseService, "/{id}"), GetAccountByIdAsync)
        .WithName("GetAccountById")
        .WithDescription("Returns a account based on id.")
        .Produces<Account>(200)
        .RequireAuthorization();

        app.MapGet(string.Join("", BaseService, "/code/{code}"), GetAccountByCodeAsync)
        .WithName("GetAccountByCode")
        .WithDescription("Returns a account based on code.")
        .Produces<Account>(200)
        .RequireAuthorization();

        app.MapPost(BaseService, InsertAccountAsync)
        .WithName("InsertAccount")
        .WithDescription("Inserts a new account.")
        .Produces<IList<Account>>(200)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPut(BaseService, UpdateAccountAsync)
        .WithName("UpdateAccount")
        .WithDescription("Updates an existing account.")
        .Produces<IList<Account>>(200)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });
    }
}
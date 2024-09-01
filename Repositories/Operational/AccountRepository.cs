using Health.Api.Data;
using Health.Api.Models.Requests.Operational;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Operational;

public interface IAccountRepository
{
    Task<IList<Account>> GetAccountsAsync();
    Task<Account?> GetAccountByIdAsync(int accountId);
    Task<Account?> GetAccountByCodeAsync(string accountCode);
    Task<Account> InsertAccountAsync(InsertAccountRequest request);
    Task<Account> UpdateAccountAsync(UpdateAccountRequest request);
    Task<int> GetAccountsCountAsync();
}

public class AccountRepository : IAccountRepository
{
    public HASDbContext _Context;

    public AccountRepository(HASDbContext context)
    {
        _Context = context;
    }

    public async Task<Account?> GetAccountByIdAsync(int accountId)
    {
        return await _Context.Accounts
                        .Where(d => d.AccountId.Equals(accountId))
                        .FirstOrDefaultAsync();
    }

    public async Task<Account?> GetAccountByCodeAsync(string accountCode)
    {
        return await _Context.Accounts
                        .Where(d => d.AccountCode.Equals(accountCode))
                        .FirstOrDefaultAsync();
    }

    public async Task<IList<Account>> GetAccountsAsync()
    {
        return await _Context.Accounts.ToListAsync();
    }

    public async Task<Account> InsertAccountAsync(InsertAccountRequest request)
    {
        Account newAccount = new()
        {
            AccountCode = request.AccountCode,
            AccountName = request.AccountName
        };

        _Context.Accounts.Add(newAccount);
        await _Context.SaveChangesAsync();
        return newAccount;
    }

    public async Task<Account> UpdateAccountAsync(UpdateAccountRequest request)
    {
        Account? existingRecord = await GetAccountByIdAsync(request.AccountId);
        if (existingRecord == null)
            throw new FileNotFoundException($"Account with id ({request.AccountId}) not found.");

        existingRecord.AccountCode = request.AccountCode;
        existingRecord.AccountName = request.AccountName;
        _Context.Accounts.Update(existingRecord);
        await _Context.SaveChangesAsync();

        return existingRecord;
    }

    public async Task<int> GetAccountsCountAsync()
    {
        return await _Context.Accounts.CountAsync();
    }
}

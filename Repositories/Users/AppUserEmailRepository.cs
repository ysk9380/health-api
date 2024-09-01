using Health.Api.Data;
using Health.Api.Models.Requests.User;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Users;

public interface IAppUserEmailRepository
{
    Task<AppUserEmail?> GetAppUserEmailByEmailAddressAsync(long appUserId, string emailAddress);
    Task<AppUserEmail?> GetAppUserEmailByIdAsync(long appUserEmailId);
    Task<IList<AppUserEmail>> GetAppUserEmailsByAppUserIdAsync(long appUserId);
    Task<AppUserEmail> InsertAppUserEmailAsync(InsertAppUserEmailRequest request);
    Task<AppUserEmail> UpdateAppUserEmailAsync(UpdateAppUserEmailRequest request);
    Task DeactivateAppUserEmailAsync(long id);
}

public class AppUserEmailRepository : IAppUserEmailRepository
{
    private HASDbContext _Context;

    public AppUserEmailRepository(HASDbContext context)
    {
        _Context = context;
    }

    public async Task<AppUserEmail?> GetAppUserEmailByEmailAddressAsync(long appUserId, string emailAddress)
    {
        return await _Context.AppUserEmails
                        .Where(p => p.AppUserId.Equals(appUserId)
                            && p.EmailAddress.Equals(emailAddress.Trim())
                            && p.IsActive)
                        .FirstOrDefaultAsync();
    }

    public async Task<AppUserEmail?> GetAppUserEmailByIdAsync(long appUserEmailId)
    {
        return await _Context.AppUserEmails
                        .Where(p => p.AppUserEmailId.Equals(appUserEmailId)
                            && p.IsActive)
                        .FirstOrDefaultAsync();
    }

    public async Task<IList<AppUserEmail>> GetAppUserEmailsByAppUserIdAsync(long appUserId)
    {
        return await _Context.AppUserEmails
                        .Where(p => p.AppUserId.Equals(appUserId) && p.IsActive)
                        .ToListAsync();
    }

    public async Task<AppUserEmail> InsertAppUserEmailAsync(InsertAppUserEmailRequest request)
    {
        var newAppUserEmail = new AppUserEmail
        {
            AppUserId = request.AppUserId,
            EmailAddress = request.EmailAddress,
            IsActive = true
        };

        _Context.AppUserEmails.Add(newAppUserEmail);
        await _Context.SaveChangesAsync();
        return newAppUserEmail;
    }

    public async Task<AppUserEmail> UpdateAppUserEmailAsync(UpdateAppUserEmailRequest request)
    {
        AppUserEmail? existingRecord = await GetAppUserEmailByIdAsync(request.AppUserEmailId);
        if (existingRecord == null)
            throw new FileNotFoundException($"App user email id ({request.AppUserEmailId}) not found.");

        existingRecord.EmailAddress = request.EmailAddress;

        _Context.Update(existingRecord);
        await _Context.SaveChangesAsync();
        return existingRecord;
    }

    public async Task DeactivateAppUserEmailAsync(long id)
    {
        AppUserEmail? existingRecord = await GetAppUserEmailByIdAsync(id);
        if (existingRecord == null)
            throw new FileNotFoundException($"App user email id ({id}) not found.");

        existingRecord.IsActive = false;

        _Context.AppUserEmails.Update(existingRecord);
        await _Context.SaveChangesAsync();
    }
}
using Health.Api.Data;
using Health.Api.Models.Requests.User;
using Health.Api.Models.User;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Users;

public interface IAppUserPhoneRepository
{
    Task<AppUserPhone?> GetAppUserPhoneByPhoneNumberAsync(long appUserId, string phoneNumber);
    Task<AppUserPhone?> GetAppUserPhoneByIdAsync(long appUserPhoneId);
    Task<IList<AppUserPhoneModel>> GetAppUserPhoneModelsByAppUserIdAsync(long appUserId);
    Task<AppUserPhoneModel?> GetAppUserPhoneModelByIdAsync(long appUserPhoneId);
    Task<IList<AppUserPhone>> GetAppUserPhonesByAppUserIdAsync(long appUserId);
    Task<AppUserPhone> InsertAppUserPhoneAsync(InsertAppUserPhoneRequest request, byte phoneTypeId);
    Task<AppUserPhone> UpdateAppUserPhoneAsync(UpdateAppUserPhoneRequest request, byte phoneTypeId);
    Task DeactivateAppUserPhoneAsync(long id);
}

public class AppUserPhoneRepository : IAppUserPhoneRepository
{
    private HASDbContext _Context;

    public AppUserPhoneRepository(HASDbContext context)
    {
        _Context = context;
    }

    public async Task<AppUserPhone?> GetAppUserPhoneByPhoneNumberAsync(long appUserId, string phoneNumber)
    {
        return await _Context.AppUserPhones
                        .Where(p => p.AppUserId.Equals(appUserId)
                                && p.PhoneNumber.Equals(phoneNumber.Trim())
                                && p.IsActive)
                        .OrderByDescending(o => o.AppUserPhoneId)
                        .FirstOrDefaultAsync();
    }

    public async Task<AppUserPhone?> GetAppUserPhoneByIdAsync(long appUserPhoneId)
    {
        return await _Context.AppUserPhones
                        .Where(p => p.AppUserPhoneId.Equals(appUserPhoneId))
                        .FirstOrDefaultAsync();
    }

    public async Task<IList<AppUserPhoneModel>> GetAppUserPhoneModelsByAppUserIdAsync(long appUserId)
    {
        return await (from pp in _Context.AppUserPhones
                      join pt in _Context.PhoneTypes on pp.PhoneTypeId equals pt.PhoneTypeId
                      where pp.AppUserId.Equals(appUserId) && pp.IsActive
                      select new AppUserPhoneModel
                      {
                          AppUserPhoneId = pp.AppUserPhoneId,
                          AppUserId = pp.AppUserId,
                          PhoneNumber = pp.PhoneNumber,
                          ListedAs = pp.ListedAs,
                          PhoneTypeCode = pt.PhoneTypeCode,
                          PhoneTypeName = pt.PhoneTypeName
                      }).ToListAsync();
    }

    public async Task<AppUserPhoneModel?> GetAppUserPhoneModelByIdAsync(long appUserPhoneId)
    {
        return await (from pp in _Context.AppUserPhones
                      join pt in _Context.PhoneTypes on pp.PhoneTypeId equals pt.PhoneTypeId
                      where pp.AppUserPhoneId.Equals(appUserPhoneId) && pp.IsActive
                      select new AppUserPhoneModel
                      {
                          AppUserPhoneId = pp.AppUserPhoneId,
                          AppUserId = pp.AppUserId,
                          PhoneNumber = pp.PhoneNumber,
                          ListedAs = pp.ListedAs,
                          PhoneTypeCode = pt.PhoneTypeCode,
                          PhoneTypeName = pt.PhoneTypeName
                      }).FirstOrDefaultAsync();
    }

    public async Task<IList<AppUserPhone>> GetAppUserPhonesByAppUserIdAsync(long appUserId)
    {
        return await _Context.AppUserPhones
                        .Where(p => p.AppUserId.Equals(appUserId) && p.IsActive)
                        .ToListAsync();
    }

    public async Task<AppUserPhone> InsertAppUserPhoneAsync(InsertAppUserPhoneRequest request
        , byte phoneTypeId)
    {
        var newAppUserPhone = new AppUserPhone
        {
            AppUserId = request.AppUserId,
            PhoneTypeId = phoneTypeId,
            PhoneNumber = request.PhoneNumber.Trim(),
            ListedAs = request.ListedAs,
            IsActive = true
        };

        _Context.AppUserPhones.Add(newAppUserPhone);
        await _Context.SaveChangesAsync();
        return newAppUserPhone;
    }

    public async Task<AppUserPhone> UpdateAppUserPhoneAsync(UpdateAppUserPhoneRequest request
        , byte phoneTypeId)
    {
        AppUserPhone? existingRecord = await GetAppUserPhoneByIdAsync(request.AppUserPhoneId);
        if (existingRecord == null)
            throw new FileNotFoundException($"App User phone id ({request.AppUserPhoneId}) not found.");

        existingRecord.PhoneNumber = request.PhoneNumber;
        existingRecord.PhoneTypeId = phoneTypeId;
        existingRecord.ListedAs = request.ListedAs;

        _Context.Update(existingRecord);
        await _Context.SaveChangesAsync();
        return existingRecord;
    }

    public async Task DeactivateAppUserPhoneAsync(long id)
    {
        AppUserPhone? existingRecord = await GetAppUserPhoneByIdAsync(id);
        if (existingRecord == null)
            throw new FileNotFoundException($"App user phone id ({id}) not found.");

        existingRecord.IsActive = false;

        _Context.AppUserPhones.Update(existingRecord);
        await _Context.SaveChangesAsync();
    }
}
using Health.Api.Data;
using Health.Api.Models.User;
using Health.Api.Models.Requests.User;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Users;

public interface IAppUserIdentityRepository
{
    Task<AppUserIdentity?> GetAppUserIdentityByIdentityTypeAndNumberAsync(long appUserId, byte identityTypeId, string identityNumber);
    Task<AppUserIdentity?> GetAppUserIdentityByIdAsync(long appUserIdentityId);
    Task<AppUserIdentityModel?> GetAppUserIdentityModelByIdAsync(long appUserIdentityId);
    Task<IList<AppUserIdentityModel>> GetAppUserIdentityModelsByAppUserIdAsync(long appUserId);
    Task<IList<AppUserIdentity>> GetAppUserIdentitiesByAppUserIdAsync(long appUserId);
    Task<AppUserIdentity> InsertAppUserIdentityAsync(InsertAppUserIdentityRequest request, byte identityTypeId);
    Task<AppUserIdentity> UpdateAppUserIdentityAsync(UpdateAppUserIdentityRequest request, byte identityTypeId);
    Task DeactivateAppUserIdentityAsync(long id);
}

public class AppUserIdentityRepository : IAppUserIdentityRepository
{
    private HASDbContext _Context;

    public AppUserIdentityRepository(HASDbContext context)
    {
        _Context = context;
    }

    public async Task<AppUserIdentity?> GetAppUserIdentityByIdentityTypeAndNumberAsync(long appUserId
        , byte identityTypeId
        , string identityNumber)
    {
        return await _Context.AppUserIdentities
                        .Where(p => p.AppUserId.Equals(appUserId)
                            && p.IdentityTypeId.Equals(identityTypeId)
                            && p.IdentityNumber.Equals(identityNumber.Trim())
                            && p.IsActive)
                        .FirstOrDefaultAsync();
    }

    public async Task<AppUserIdentity?> GetAppUserIdentityByIdAsync(long appUserIdentityId)
    {
        return await _Context.AppUserIdentities
                        .Where(p => p.AppUserIdentityId.Equals(appUserIdentityId)
                            && p.IsActive)
                        .FirstOrDefaultAsync();
    }

    public async Task<AppUserIdentityModel?> GetAppUserIdentityModelByIdAsync(long appUserIdentityId)
    {
        return await (from pi in _Context.AppUserIdentities
                      join it in _Context.IdentityTypes on pi.IdentityTypeId equals it.IdentityTypeId
                      where pi.AppUserIdentityId.Equals(appUserIdentityId) && pi.IsActive
                      select new AppUserIdentityModel
                      {
                          AppUserIdentityId = pi.AppUserIdentityId,
                          AppUserId = pi.AppUserId,
                          IdentityNumber = pi.IdentityNumber,
                          IssuedBy = pi.IssuedBy,
                          PlaceIssued = pi.PlaceIssued,
                          Expiry = pi.Expiry,
                          IdentityTypeCode = it.IdentityTypeCode,
                          IdentityTypeName = it.IdentityTypeName
                      }).FirstOrDefaultAsync();
    }

    public async Task<IList<AppUserIdentityModel>> GetAppUserIdentityModelsByAppUserIdAsync(long appUserId)
    {
        return await (from pi in _Context.AppUserIdentities
                      join it in _Context.IdentityTypes on pi.IdentityTypeId equals it.IdentityTypeId
                      where pi.AppUserId.Equals(appUserId) && pi.IsActive
                      select new AppUserIdentityModel
                      {
                          AppUserIdentityId = pi.AppUserIdentityId,
                          AppUserId = pi.AppUserId,
                          IdentityNumber = pi.IdentityNumber,
                          IssuedBy = pi.IssuedBy,
                          PlaceIssued = pi.PlaceIssued,
                          Expiry = pi.Expiry,
                          IdentityTypeCode = it.IdentityTypeCode,
                          IdentityTypeName = it.IdentityTypeName
                      }).ToListAsync();
    }

    public async Task<IList<AppUserIdentity>> GetAppUserIdentitiesByAppUserIdAsync(long appUserId)
    {
        return await _Context.AppUserIdentities
                        .Where(p => p.AppUserId.Equals(appUserId) && p.IsActive)
                        .ToListAsync();
    }

    public async Task<AppUserIdentity> InsertAppUserIdentityAsync(InsertAppUserIdentityRequest request, byte identityTypeId)
    {
        var newAppUserIdentity = new AppUserIdentity
        {
            AppUserId = request.AppUserId,
            IdentityTypeId = identityTypeId,
            IdentityNumber = request.IdentityNumber,
            IssuedBy = request.IssuedBy,
            PlaceIssued = request.PlaceIssued,
            Expiry = request.Expiry,
            IsActive = true
        };

        _Context.AppUserIdentities.Add(newAppUserIdentity);
        await _Context.SaveChangesAsync();
        return newAppUserIdentity;
    }

    public async Task<AppUserIdentity> UpdateAppUserIdentityAsync(UpdateAppUserIdentityRequest request, byte identityTypeId)
    {
        AppUserIdentity? existingRecord = await GetAppUserIdentityByIdAsync(request.AppUserIdentityId);
        if (existingRecord == null)
            throw new FileNotFoundException($"AppUser identity id ({request.AppUserIdentityId}) not found.");

        existingRecord.IdentityTypeId = identityTypeId;
        existingRecord.IdentityNumber = request.IdentityNumber;
        existingRecord.IssuedBy = request.IssuedBy;
        existingRecord.PlaceIssued = request.PlaceIssued;
        existingRecord.Expiry = request.Expiry;

        _Context.Update(existingRecord);
        await _Context.SaveChangesAsync();
        return existingRecord;
    }

    public async Task DeactivateAppUserIdentityAsync(long id)
    {
        AppUserIdentity? existingRecord = await GetAppUserIdentityByIdAsync(id);
        if (existingRecord == null)
            throw new FileNotFoundException($"AppUser identity id ({id}) not found.");

        existingRecord.IsActive = false;

        _Context.AppUserIdentities.Update(existingRecord);
        await _Context.SaveChangesAsync();
    }
}
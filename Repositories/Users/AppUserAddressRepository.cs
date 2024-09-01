using Health.Api.Data;
using Health.Api.Models.Requests.User;
using Health.Api.Models.User;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Users;

public interface IAppUserAddressRepository
{
    Task<AppUserAddress?> GetAppUserAddressByAddressTypeAsync(long appUserId, byte addressTypeId);
    Task<AppUserAddress?> GetAppUserAddressByIdAsync(long appUserAddressId);
    Task<AppUserAddressModel?> GetAppUserAddressModelByIdAsync(long appUserAddressId);
    Task<IList<AppUserAddressModel>> GetAppUserAddressModelsByAppUserIdAsync(long appUserId);
    Task<IList<AppUserAddress>> GetAppUserAddressesByAppUserIdAsync(long appUserId);
    Task<AppUserAddress> InsertAppUserAddressAsync(InsertAppUserAddressRequest request, byte addressTypeId, byte stateId);
    Task<AppUserAddress> UpdateAppUserAddressRecordAsync(UpdateAppUserAddressRequest request, byte addressTypeId, byte stateId);
    Task DeactivateAppUserAddressAsync(long id);
}

public class AppUserAddressRepository : IAppUserAddressRepository
{
    private HASDbContext _Context;

    public AppUserAddressRepository(HASDbContext context)
    {
        _Context = context;
    }

    public async Task<AppUserAddress?> GetAppUserAddressByAddressTypeAsync(long appUserId, byte addressTypeId)
    {
        AppUserAddress? address = await _Context.AppUserAddresses
                                    .FirstOrDefaultAsync(a => a.AppUserId.Equals(appUserId)
                                        && a.AddressTypeId.Equals(addressTypeId)
                                        && a.IsActive);
        return address;
    }

    public async Task<AppUserAddress?> GetAppUserAddressByIdAsync(long appUserAddressId)
    {
        return await _Context.AppUserAddresses
                        .Where(p => p.AppUserAddressId.Equals(appUserAddressId))
                        .FirstOrDefaultAsync();
    }

    public async Task<AppUserAddressModel?> GetAppUserAddressModelByIdAsync(long appUserAddressId)
    {
        return await (from aua in _Context.AppUserAddresses
                      join at in _Context.AddressTypes on aua.AddressTypeId equals at.AddressTypeId
                      join s in _Context.States on aua.StateId equals s.StateId
                      where aua.AppUserAddressId.Equals(appUserAddressId) && aua.IsActive
                      select new AppUserAddressModel
                      {
                          AppUserAddressId = aua.AppUserAddressId,
                          AppUserId = aua.AppUserId,
                          AddressLine1 = aua.AddressLine1,
                          AddressLine2 = aua.AddressLine2,
                          AddressLine3 = aua.AddressLine3,
                          City = aua.City,
                          Pincode = aua.Pincode,
                          StateCode = s.StateCode,
                          StateName = s.StateName,
                          AddressTypeCode = at.AddressTypeCode,
                          AddressTypeName = at.AddressTypeName,
                      }).FirstOrDefaultAsync();
    }

    public async Task<IList<AppUserAddressModel>> GetAppUserAddressModelsByAppUserIdAsync(long appUserId)
    {
        return await (from aua in _Context.AppUserAddresses
                      join at in _Context.AddressTypes on aua.AddressTypeId equals at.AddressTypeId
                      join s in _Context.States on aua.StateId equals s.StateId
                      where aua.AppUserId.Equals(appUserId) && aua.IsActive
                      select new AppUserAddressModel
                      {
                          AppUserAddressId = aua.AppUserAddressId,
                          AppUserId = aua.AppUserId,
                          AddressLine1 = aua.AddressLine1,
                          AddressLine2 = aua.AddressLine2,
                          AddressLine3 = aua.AddressLine3,
                          City = aua.City,
                          Pincode = aua.Pincode,
                          StateCode = s.StateCode,
                          StateName = s.StateName,
                          AddressTypeCode = at.AddressTypeCode,
                          AddressTypeName = at.AddressTypeName,
                      }).ToListAsync();
    }

    public async Task<IList<AppUserAddress>> GetAppUserAddressesByAppUserIdAsync(long appUserId)
    {
        return await _Context.AppUserAddresses
                        .Where(p => p.AppUserId.Equals(appUserId) && p.IsActive)
                        .ToListAsync();
    }

    public async Task<AppUserAddress> InsertAppUserAddressAsync(InsertAppUserAddressRequest request
        , byte addressTypeId
        , byte stateId)
    {
        var newAppUserAddress = new AppUserAddress
        {
            AppUserId = request.AppUserId,
            AddressTypeId = addressTypeId,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            AddressLine3 = request.AddressLine3,
            City = request.City,
            Pincode = request.Pincode,
            StateId = stateId,
            IsActive = true
        };

        _Context.AppUserAddresses.Add(newAppUserAddress);
        await _Context.SaveChangesAsync();
        return newAppUserAddress;
    }

    public async Task<AppUserAddress> UpdateAppUserAddressRecordAsync(UpdateAppUserAddressRequest request, byte addressTypeId, byte stateId)
    {
        AppUserAddress? existingRecord = await GetAppUserAddressByIdAsync(request.AppUserAddressId);
        if (existingRecord == null)
            throw new FileNotFoundException($"App user address id ({request.AppUserAddressId}) not found.");

        existingRecord.AddressTypeId = addressTypeId;
        existingRecord.AddressLine1 = request.AddressLine1;
        existingRecord.AddressLine2 = request.AddressLine2;
        existingRecord.AddressLine3 = request.AddressLine3;
        existingRecord.City = request.City;
        existingRecord.Pincode = request.Pincode;
        existingRecord.StateId = stateId;

        _Context.AppUserAddresses.Update(existingRecord);
        await _Context.SaveChangesAsync();

        return existingRecord;
    }

    public async Task DeactivateAppUserAddressAsync(long id)
    {
        AppUserAddress? existingRecord = await GetAppUserAddressByIdAsync(id);
        if (existingRecord == null)
            throw new FileNotFoundException($"App User address id ({id}) not found.");

        existingRecord.IsActive = false;

        _Context.AppUserAddresses.Update(existingRecord);
        await _Context.SaveChangesAsync();
    }
}
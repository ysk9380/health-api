using Health.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories;

public interface IMasterRepository
{
    Task<IList<Market>> GetMarketsAsync();
    Task<IList<Gender>> GetGendersAsync();
    Task<byte> GetGenderIdAsync(string genderCode);
    Task<IList<AddressType>> GetAddressTypesAsync();
    Task<byte> GetAddressTypeIdAsync(string addressTypeCode);
    Task<IList<PhoneType>> GetPhoneTypesAsync();
    Task<byte> GetPhoneTypeIdAsync(string phoneTypeCode);
    Task<IList<IdentityType>> GetIdentityTypesAsync();
    Task<byte> GetIdentityTypeIdAsync(string identityTypeCode);
    Task<IList<State>> GetStatesAsync();
    Task<byte> GetStateIdAsync(string stateCode);
    Task<IList<Language>> GetLanguagesAsync();
    Task<byte> GetLanguageIdAsync(string languageCode);
    Task<IList<AppUserRole>> GetAppUserRolesAsync();
    Task<byte> GetAppUserRoleIdAsync(string roleCode);
    Task<IList<ServiceCategory>> GetServiceCategoriesAsync();
    Task<byte> GetServiceCategoryIdAsync(string serviceCategoryCode);
    Task<IList<HealthVital>> GetHealthVitalsAsync();
    Task<int> GetHealthVitalIdAsync(string healthVitalCode);
}

public class MasterRepository : IMasterRepository
{
    private HASDbContext _Context;

    public MasterRepository(HASDbContext context)
    {
        _Context = context;
    }

    public async Task<IList<Market>> GetMarketsAsync()
    {
        return await _Context.Markets.Where(m => m.IsActive)
                        .OrderBy(o => o.MarketId)
                        .ToListAsync();

    }

    public async Task<IList<Gender>> GetGendersAsync()
    {
        return await _Context.Genders
                        .OrderBy(o => o.GenderId)
                        .ToListAsync();
    }

    public async Task<byte> GetGenderIdAsync(string genderCode)
    {
        byte genderId = await _Context.Genders.Where(g => g.GenderCode.Equals(genderCode))
                                .Select(s => s.GenderId)
                                .FirstOrDefaultAsync();
        return genderId;
    }

    public async Task<IList<AddressType>> GetAddressTypesAsync()
    {
        return await _Context.AddressTypes
                        .OrderBy(o => o.AddressTypeId)
                        .ToListAsync();
    }

    public async Task<byte> GetAddressTypeIdAsync(string addressTypeCode)
    {
        byte addressTypeId = await _Context.AddressTypes.Where(g => g.AddressTypeCode.Equals(addressTypeCode))
                                        .Select(s => s.AddressTypeId)
                                        .FirstOrDefaultAsync();
        return addressTypeId;
    }

    public async Task<IList<PhoneType>> GetPhoneTypesAsync()
    {
        return await _Context.PhoneTypes
                        .OrderBy(o => o.PhoneTypeId)
                        .ToListAsync();
    }

    public async Task<byte> GetPhoneTypeIdAsync(string phoneTypeCode)
    {
        byte phoneTypeId = await _Context.PhoneTypes.Where(g => g.PhoneTypeCode.Equals(phoneTypeCode))
                                        .Select(s => s.PhoneTypeId)
                                        .FirstOrDefaultAsync();
        return phoneTypeId;
    }

    public async Task<IList<IdentityType>> GetIdentityTypesAsync()
    {
        return await _Context.IdentityTypes
                        .OrderBy(o => o.IdentityTypeId)
                        .ToListAsync();
    }

    public async Task<byte> GetIdentityTypeIdAsync(string identityTypeCode)
    {
        byte identityTypeId = await _Context.IdentityTypes
                                        .Where(i => i.IdentityTypeCode.Equals(identityTypeCode))
                                        .Select(s => s.IdentityTypeId)
                                        .FirstOrDefaultAsync();
        return identityTypeId;
    }

    public async Task<IList<State>> GetStatesAsync()
    {
        return await _Context.States
                        .OrderBy(o => o.StateId)
                        .ToListAsync();
    }

    public async Task<byte> GetStateIdAsync(string stateCode)
    {
        byte stateId = await _Context.States.Where(g => g.StateCode.Equals(stateCode))
                                        .Select(s => s.StateId)
                                        .FirstOrDefaultAsync();
        return stateId;
    }

    public async Task<IList<Language>> GetLanguagesAsync()
    {
        return await _Context.Languages
                        .OrderBy(o => o.LanguageId)
                        .ToListAsync();
    }

    public async Task<byte> GetLanguageIdAsync(string languageCode)
    {
        byte languageId = await _Context.Languages.Where(g => g.LanguageCode.Equals(languageCode))
                                        .Select(s => s.LanguageId)
                                        .FirstOrDefaultAsync();
        return languageId;
    }

    public async Task<IList<AppUserRole>> GetAppUserRolesAsync()
    {
        return await _Context.AppUserRoles
                        .OrderBy(o => o.AppUserRoleId)
                        .ToListAsync();
    }

    public async Task<byte> GetAppUserRoleIdAsync(string roleCode)
    {
        byte languageId = await _Context.AppUserRoles.Where(r => r.AppUserRoleCode.Equals(roleCode))
                                        .Select(s => s.AppUserRoleId)
                                        .FirstOrDefaultAsync();
        return languageId;
    }

    public async Task<IList<ServiceCategory>> GetServiceCategoriesAsync()
    {
        return await _Context.ServiceCategories
                        .OrderBy(o => o.ServiceCategoryId)
                        .ToListAsync();
    }

    public async Task<byte> GetServiceCategoryIdAsync(string serviceCategoryCode)
    {
        byte serviceCategoryId = await _Context.ServiceCategories.Where(r => r.ServiceCategoryCode.Equals(serviceCategoryCode))
                                        .Select(s => s.ServiceCategoryId)
                                        .FirstOrDefaultAsync();
        return serviceCategoryId;
    }

    public async Task<IList<HealthVital>> GetHealthVitalsAsync()
    {
        return await _Context.HealthVitals
                        .OrderBy(o => o.HealthVitalId)
                        .ToListAsync();
    }

    public async Task<int> GetHealthVitalIdAsync(string healthVitalCode)
    {
        int healthVitalId = await _Context.HealthVitals.Where(r => r.HealthVitalCode.Equals(healthVitalCode))
                                        .Select(s => s.HealthVitalId)
                                        .FirstOrDefaultAsync();
        return healthVitalId;
    }
}
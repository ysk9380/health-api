using Health.Api.Authentication;
using Health.Api.Data;
using Health.Api.Models.Requests;
using Health.Api.Models.Requests.User;
using Health.Api.Models.Responses;
using Health.Api.Models.User;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Users;

public interface IAppUserRepository
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<bool> UpdateTokenAsync(long customerId, long appUserId, string? token);
    Task<CustomerAppUser?> GetCustomerAppUserAsync(string customerCode, string username, string password);
    Task<bool> UpdateCustomerAppUserPasswordAsync(long customerAppUserId, string password);
    Task<bool> UpdateCustomerAppUserLanguageAsync(long customerId, long appUserId, byte languageId);
    Task<LoginResponse?> GetLoginResponseUsingRefreshTokenIdAsync(string refreshTokenId);
    Task<AppUser?> GetAppUserByNameAndDoBAsync(string firstname, string lastname, DateTime dateOfBirth);
    Task<AppUser?> GetOtherAppUserWithSameNameAndDoBAsync(string firstname, string lastname, DateTime dateOfBirth, long appUserId);
    Task<AppUser> InsertNewAppUserAsync(InsertAppUserRequest request, byte genderId);
    Task<CustomerAppUser?> GetCustomerAppUserAsync(long customerId, long appUserId);
    Task<CustomerAppUser> InsertCustomerAppUserAsync(CreateLoginRequest request, byte languageId, byte appUserRoleId);
    Task<CustomerAppUser> ActivateCustomerAppUserAsync(CreateLoginRequest request, long customerAppUserId, byte appUserRoleId);
    Task<bool> ActivateCustomerAppUserAsync(long customerAppUserId);
    Task<bool> DeactivateCustomerAppUserAsync(long customerAppUserId);
    Task<CustomerAppUser?> GetCustomerAppUserByUsernameAsync(long customerId, string username);
    Task<int> GetAppUsersCountAsync();
    Task<IList<AppUserSearchModel>> SearchAppUsersAsync(AppUserSearchRequest request);
    Task<AppUserModel?> GetAppUserModelAsync(long appUserId);
    Task<AppUser?> GetAppUserAsync(long appUserId);
    Task<AppUser> UpdateAppUserAsync(UpdateAppUserRequest request, byte genderId);
    Task<IList<CustomerAppUserModel>> GetCustomerAppUsersAsync(long customerId);
    Task<CustomerAppUser?> GetCustomerAppUserAsync(long customerAppUserId);
}

public class AppUserRepository : IAppUserRepository
{
    private HASDbContext _Context;
    private IDataAuditHistoryRepository _DataAuditHistoryRepository;
    private const string Key = "obbgtpogpkigupog";

    public AppUserRepository(HASDbContext context
        , IDataAuditHistoryRepository dataAuditHistoryRepository)
    {
        _Context = context;
        _DataAuditHistoryRepository = dataAuditHistoryRepository;
    }

    async Task<LoginResponse?> IAppUserRepository.LoginAsync(LoginRequest request)
    {
        string encryptedPassword = PasswordManager.EncryptString(request.Password, Key);
        LoginResponse? response = await (from appUser in _Context.AppUsers
                                         join customerAppUser in _Context.CustomerAppUsers on appUser.AppUserId equals customerAppUser.AppUserId
                                         join customer in _Context.Customers on customerAppUser.CustomerId equals customer.CustomerId
                                         join lang in _Context.Languages on customerAppUser.LanguageId equals lang.LanguageId
                                         join role in _Context.AppUserRoles on customerAppUser.AppUserRoleId equals role.AppUserRoleId
                                         where customerAppUser.Username.Equals(request.Username)
                                             && customer.CustomerCode.Equals(request.CustomerCode)
                                             && customerAppUser.IsActive
                                         select new LoginResponse
                                         {
                                             Username = customerAppUser.Username,
                                             Secret = customerAppUser.Password,
                                             AppUserId = appUser.AppUserId,
                                             Firstname = appUser.Firstname,
                                             Lastname = appUser.Lastname,
                                             CustomerId = customer.CustomerId,
                                             CustomerCode = customer.CustomerCode,
                                             CustomerShortName = customer.CustomerShortName,
                                             CustomerName = customer.CustomerName,
                                             LanguageCode = lang.LanguageCode,
                                             RoleCode = role.AppUserRoleCode,
                                         }).FirstOrDefaultAsync();

        if (response != null)
        {
            string decryptedSecret = PasswordManager.DecryptToString(response.Secret, Key);
            if (string.Equals(request.Password, decryptedSecret))
                return response;
            return null;
        }

        return response;

    }

    public async Task<bool> UpdateTokenAsync(long customerId, long appUserId, string? token)
    {
        bool result = false;
        CustomerAppUser? customerAppUser = await _Context.CustomerAppUsers
                                                .Where(cau => cau.CustomerId.Equals(customerId)
                                                    && cau.AppUserId.Equals(appUserId)
                                                    && cau.IsActive)
                                                .FirstOrDefaultAsync();


        if (customerAppUser != null)
        {
            CustomerAppUser customerAppUserCopy = new CustomerAppUser(customerAppUser);

            customerAppUser.Token = token;
            _Context.CustomerAppUsers.Update(customerAppUser);
            await _Context.SaveChangesAsync();
            result = true;

            await _DataAuditHistoryRepository.RecordEditModelAuditDataAsync<CustomerAppUser>
                (customerAppUserCopy
                , customerAppUser
                , "customerappuser"
                , customerAppUser.CustomerAppUserId
                , customerId
                , appUserId);
        }

        return result;
    }

    public async Task<CustomerAppUser?> GetCustomerAppUserAsync(string customerCode, string username, string password)
    {
        CustomerAppUser? customerAppUser = await (from cau in _Context.CustomerAppUsers
                                                  join c in _Context.Customers on cau.CustomerId equals c.CustomerId
                                                  where c.CustomerCode.Equals(customerCode)
                                                      && cau.Username.Equals(username)
                                                  select cau).FirstOrDefaultAsync();

        if (customerAppUser != null)
        {
            string decryptedSecret = PasswordManager.DecryptToString(customerAppUser.Password, Key);
            if (string.Equals(password, decryptedSecret))
                return customerAppUser;
            return null;
        }

        return customerAppUser;
    }

    public async Task<bool> UpdateCustomerAppUserPasswordAsync(long customerAppUserId, string password)
    {
        CustomerAppUser? customerAppUser = await _Context.CustomerAppUsers.Where(cau => cau.CustomerAppUserId.Equals(customerAppUserId)).FirstOrDefaultAsync();
        if (customerAppUser != null)
        {
            CustomerAppUser customerAppUserCopy = new CustomerAppUser(customerAppUser);

            customerAppUser.Password = PasswordManager.EncryptString(password, Key);
            _Context.CustomerAppUsers.Update(customerAppUser);
            await _Context.SaveChangesAsync();

            await _DataAuditHistoryRepository.RecordEditModelAuditDataAsync<CustomerAppUser>
                (customerAppUserCopy
                , customerAppUser
                , "customerappuser"
                , customerAppUser.CustomerAppUserId
                , customerAppUser.CustomerId
                , customerAppUser.AppUserId);

            return true;
        }

        return false;
    }

    public async Task<bool> UpdateCustomerAppUserLanguageAsync(long customerId, long appUserId, byte languageId)
    {
        bool result = false;
        CustomerAppUser? customerAppUser = await _Context.CustomerAppUsers
                                                .Where(cau => cau.CustomerId.Equals(customerId)
                                                    && cau.AppUserId.Equals(appUserId)
                                                    && cau.IsActive)
                                                .FirstOrDefaultAsync();
        if (customerAppUser != null)
        {
            CustomerAppUser customerAppUserCopy = new CustomerAppUser(customerAppUser);

            customerAppUser.LanguageId = languageId;
            _Context.CustomerAppUsers.Update(customerAppUser);
            await _Context.SaveChangesAsync();
            result = true;

            await _DataAuditHistoryRepository.RecordEditModelAuditDataAsync<CustomerAppUser>
                (customerAppUserCopy
                , customerAppUser
                , "customerappuser"
                , customerAppUser.CustomerAppUserId
                , customerId
                , appUserId);
        }

        return result;
    }

    public async Task<LoginResponse?> GetLoginResponseUsingRefreshTokenIdAsync(string refreshTokenId)
    {
        return await (from appUser in _Context.AppUsers
                      join customerAppUser in _Context.CustomerAppUsers on appUser.AppUserId equals customerAppUser.AppUserId
                      join customer in _Context.Customers on customerAppUser.CustomerId equals customer.CustomerId
                      join lang in _Context.Languages on customerAppUser.LanguageId equals lang.LanguageId
                      join role in _Context.AppUserRoles on customerAppUser.AppUserRoleId equals role.AppUserRoleId
                      where string.Equals(customerAppUser.Token, refreshTokenId)
                          && customerAppUser.IsActive
                      select new LoginResponse
                      {
                          AppUserId = appUser.AppUserId,
                          Username = customerAppUser.Username,
                          Firstname = appUser.Firstname,
                          Lastname = appUser.Lastname,
                          CustomerId = customer.CustomerId,
                          CustomerCode = customer.CustomerCode,
                          CustomerShortName = customer.CustomerShortName,
                          CustomerName = customer.CustomerName,
                          LanguageCode = lang.LanguageCode,
                          RoleCode = role.AppUserRoleCode,
                      }).FirstOrDefaultAsync();
    }

    public async Task<AppUser?> GetAppUserByNameAndDoBAsync(string firstname, string lastname, DateTime dateOfBirth)
    {
        return await _Context.AppUsers.FirstOrDefaultAsync(p => p.Firstname.Equals(firstname)
                                && p.Lastname.Equals(lastname)
                                && p.DateOfBirth.Equals(dateOfBirth));
    }

    public async Task<AppUser?> GetOtherAppUserWithSameNameAndDoBAsync(string firstname
        , string lastname
        , DateTime dateOfBirth
        , long appUserId)
    {
        return await _Context.AppUsers.FirstOrDefaultAsync(p => p.Firstname.Equals(firstname)
                        && p.Lastname.Equals(lastname)
                        && p.DateOfBirth.Equals(dateOfBirth)
                        && !p.AppUserId.Equals(appUserId));
    }

    public async Task<AppUser> InsertNewAppUserAsync(InsertAppUserRequest request, byte genderId)
    {
        var newAppUser = new AppUser
        {
            Firstname = request.Firstname.Trim(),
            Middlename = request.Middlename?.Trim(),
            Lastname = request.Lastname.Trim(),
            DateOfBirth = request.DateOfBirth,
            GenderId = genderId
        };

        _Context.AppUsers.Add(newAppUser);
        await _Context.SaveChangesAsync();

        return newAppUser;
    }

    public async Task<CustomerAppUser?> GetCustomerAppUserAsync(long customerId, long appUserId)
    {
        return await _Context.CustomerAppUsers
                        .Where(cau => cau.CustomerId.Equals(customerId)
                            && cau.AppUserId.Equals(appUserId))
                        .FirstOrDefaultAsync();
    }

    public async Task<CustomerAppUser> InsertCustomerAppUserAsync(CreateLoginRequest request
        , byte languageId
        , byte appUserRoleId)
    {
        var newCustomerAppUser = new CustomerAppUser
        {
            CustomerId = request.CustomerId,
            AppUserId = request.AppUserId,
            Username = request.EmailAddress,
            Password = PasswordManager.EncryptString(request.Password, Key),
            AppUserRoleId = appUserRoleId,
            LanguageId = languageId,
            IsActive = true
        };

        _Context.CustomerAppUsers.Add(newCustomerAppUser);
        await _Context.SaveChangesAsync();
        return newCustomerAppUser;
    }

    public async Task<CustomerAppUser> ActivateCustomerAppUserAsync(CreateLoginRequest request
        , long customerAppUserId
        , byte appUserRoleId)
    {
        CustomerAppUser existingCustomerAppUser
            = await _Context.CustomerAppUsers
                        .Where(cau => cau.CustomerAppUserId.Equals(customerAppUserId))
                        .FirstAsync();

        existingCustomerAppUser.Username = request.EmailAddress;
        existingCustomerAppUser.Password = PasswordManager.EncryptString(request.Password, Key);
        existingCustomerAppUser.AppUserRoleId = appUserRoleId;
        existingCustomerAppUser.Token = null;
        existingCustomerAppUser.IsActive = true;
        _Context.CustomerAppUsers.Update(existingCustomerAppUser);
        await _Context.SaveChangesAsync();

        return existingCustomerAppUser;
    }

    public async Task<bool> ActivateCustomerAppUserAsync(long customerAppUserId)
    {
        CustomerAppUser existingCustomerAppUser
                    = await _Context.CustomerAppUsers
                                .Where(cau => cau.CustomerAppUserId.Equals(customerAppUserId))
                                .FirstAsync();
        existingCustomerAppUser.IsActive = true;
        _Context.CustomerAppUsers.Update(existingCustomerAppUser);
        int updateCount = await _Context.SaveChangesAsync();
        return updateCount > 0;
    }

    public async Task<bool> DeactivateCustomerAppUserAsync(long customerAppUserId)
    {
        CustomerAppUser? existingCustomerAppUser
            = await _Context.CustomerAppUsers
                        .Where(cau => cau.CustomerAppUserId.Equals(customerAppUserId))
                        .FirstOrDefaultAsync();

        if (existingCustomerAppUser == null)
        {
            return false;
        }
        else
        {
            existingCustomerAppUser.Token = null;
            existingCustomerAppUser.IsActive = false;
            _Context.CustomerAppUsers.Update(existingCustomerAppUser);
            await _Context.SaveChangesAsync();
            return true;
        }
    }

    public async Task<CustomerAppUser?> GetCustomerAppUserByUsernameAsync(long customerId, string username)
    {
        return await _Context.CustomerAppUsers
                        .Where(cau => cau.CustomerId.Equals(customerId)
                            && cau.Username.Equals(username))
                        .FirstOrDefaultAsync();
    }

    public async Task<int> GetAppUsersCountAsync()
    {
        return await _Context.AppUsers.CountAsync();
    }

    public async Task<IList<AppUserSearchModel>> SearchAppUsersAsync(AppUserSearchRequest request)
    {
        var appUsersQuery = _Context.AppUsers.AsQueryable<AppUser>();
        var appUserPhonesQuery = _Context.AppUserPhones.Where(ph => ph.IsActive).AsQueryable<AppUserPhone>();
        var appUserEmailsQuery = _Context.AppUserEmails.Where(pe => pe.IsActive).AsQueryable<AppUserEmail>();
        var appUserIdentityQuery = _Context.AppUserIdentities.Where(pi => pi.IsActive).AsQueryable<AppUserIdentity>();

        var searchQuery = (from p in appUsersQuery
                           join ph in appUserPhonesQuery on p.AppUserId equals ph.AppUserId into appUserPhoneGroup

                           from ppg in appUserPhoneGroup.DefaultIfEmpty()
                           join pe in appUserEmailsQuery on p.AppUserId equals pe.AppUserId into appUserEmailGroup

                           from peg in appUserEmailGroup.DefaultIfEmpty()
                           join pi in appUserIdentityQuery on p.AppUserId equals pi.AppUserId into appUserIdentityGroup

                           from pig in appUserIdentityGroup.DefaultIfEmpty()
                           join g in _Context.Genders on p.GenderId equals g.GenderId
                           select new
                           {
                               AppUserId = p.AppUserId,
                               Firstname = p.Firstname,
                               Middlename = p.Middlename,
                               Lastname = p.Lastname,
                               DateOfBirth = p.DateOfBirth,
                               Gender = g.GenderName,
                               PhoneNumber = ppg.PhoneNumber,
                               EmailAddress = peg.EmailAddress,
                               IdentityNumber = pig.IdentityNumber
                           }).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Firstname))
            searchQuery = searchQuery.Where(p => EF.Functions.Like(p.Firstname, $"{request.Firstname}%"));

        if (!string.IsNullOrWhiteSpace(request.Lastname))
            searchQuery = searchQuery.Where(p => EF.Functions.Like(p.Lastname, $"{request.Lastname}%"));

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            searchQuery = searchQuery.Where(p => p.PhoneNumber.Equals(request.PhoneNumber));

        if (!string.IsNullOrWhiteSpace(request.Email))
            searchQuery = searchQuery.Where(e => e.EmailAddress.Equals(request.Email));

        if (!string.IsNullOrWhiteSpace(request.IdentityNumber))
            searchQuery = searchQuery.Where(e => e.IdentityNumber.Equals(request.IdentityNumber));

        IList<AppUserSearchModel> searchResult = await searchQuery.Select(s => new AppUserSearchModel
        {
            AppUserId = s.AppUserId,
            Firstname = s.Firstname,
            Middlename = s.Middlename,
            Lastname = s.Lastname,
            DateOfBirth = s.DateOfBirth,
            Gender = s.Gender,
            PhoneNumber = s.PhoneNumber,
            EmailAddress = s.EmailAddress,
            IdentityNumber = s.IdentityNumber
        })
        .Take(100)
        .ToListAsync();

        IList<AppUserSearchModel> groupedSearchResult = GetGroupedAppUserRecords(searchResult)
        .ToList();

        return groupedSearchResult;
    }

    public async Task<AppUserModel?> GetAppUserModelAsync(long appUserId)
    {
        return await (from a in _Context.AppUsers
                      join g in _Context.Genders on a.GenderId equals g.GenderId
                      where a.AppUserId.Equals(appUserId)
                      select new AppUserModel
                      {
                          AppUserId = a.AppUserId,
                          Firstname = a.Firstname,
                          Middlename = a.Middlename,
                          Lastname = a.Lastname,
                          DateOfBirth = a.DateOfBirth,
                          GenderId = a.GenderId,
                          GenderCode = g.GenderCode,
                          GenderName = g.GenderName
                      }).FirstOrDefaultAsync();
    }

    public async Task<AppUser?> GetAppUserAsync(long appUserId)
    {
        return await _Context.AppUsers
                        .Where(a => a.AppUserId.Equals(appUserId))
                        .FirstOrDefaultAsync();
    }

    public async Task<AppUser> UpdateAppUserAsync(UpdateAppUserRequest request, byte genderId)
    {
        AppUser? existingAppUser = await _Context.AppUsers
                                    .Where(a => a.AppUserId.Equals(request.AppUserId))
                                    .FirstOrDefaultAsync();

        if (existingAppUser == null)
            throw new FileNotFoundException($"App User id ({request.AppUserId}) not found.");


        existingAppUser.Firstname = request.Firstname;
        existingAppUser.Middlename = request.Middlename;
        existingAppUser.Lastname = request.Lastname;
        existingAppUser.DateOfBirth = request.DateOfBirth;
        existingAppUser.GenderId = genderId;

        _Context.AppUsers.Update(existingAppUser);
        await _Context.SaveChangesAsync();

        return existingAppUser;
    }

    public async Task<IList<CustomerAppUserModel>> GetCustomerAppUsersAsync(long customerId)
    {
        IList<CustomerAppUserModel> customerAppUsers = await (from cau in _Context.CustomerAppUsers
                                                              join c in _Context.Customers on cau.CustomerId equals c.CustomerId
                                                              join au in _Context.AppUsers on cau.AppUserId equals au.AppUserId
                                                              join g in _Context.Genders on au.GenderId equals g.GenderId
                                                              join aur in _Context.AppUserRoles on cau.AppUserRoleId equals aur.AppUserRoleId
                                                              join l in _Context.Languages on cau.LanguageId equals l.LanguageId
                                                              where cau.CustomerId.Equals(customerId)
                                                              orderby cau.IsActive descending, cau.CustomerAppUserId descending
                                                              select new CustomerAppUserModel
                                                              {
                                                                  CustomerAppUserId = cau.CustomerAppUserId,
                                                                  CustomerId = cau.CustomerId,
                                                                  CustomerCode = c.CustomerCode,
                                                                  CustomerShortName = c.CustomerShortName,
                                                                  CustomerName = c.CustomerName,
                                                                  AppUserId = cau.AppUserId,
                                                                  Firstname = au.Firstname,
                                                                  Middlename = au.Middlename,
                                                                  Lastname = au.Lastname,
                                                                  DateOfBirth = au.DateOfBirth,
                                                                  GenderId = au.GenderId,
                                                                  GenderCode = g.GenderCode,
                                                                  GenderName = g.GenderName,
                                                                  Username = cau.Username,
                                                                  AppUserRoleId = cau.AppUserRoleId,
                                                                  AppUserRoleCode = aur.AppUserRoleCode,
                                                                  AppUserRoleName = aur.AppUserRoleName,
                                                                  LanguageId = cau.LanguageId,
                                                                  LanguageCode = l.LanguageCode,
                                                                  LanguageName = l.LanguageName,
                                                                  IsActive = cau.IsActive

                                                              }).ToListAsync();
        return customerAppUsers;
    }

    public async Task<CustomerAppUser?> GetCustomerAppUserAsync(long customerAppUserId)
    {
        return await _Context.CustomerAppUsers
                        .Where(cau => cau.CustomerAppUserId.Equals(customerAppUserId))
                        .FirstOrDefaultAsync();
    }

    #region Private Methods

    private IEnumerable<AppUserSearchModel> GetGroupedAppUserRecords(IList<AppUserSearchModel> patientRecords)
    {
        return patientRecords.GroupBy(g => g.AppUserId).Select(s => new AppUserSearchModel
        {
            AppUserId = s.Key,
            Firstname = s.First().Firstname,
            Middlename = s.First().Middlename,
            Lastname = s.First().Lastname,
            DateOfBirth = s.First().DateOfBirth,
            Gender = s.First().Gender,
            PhoneNumber = s.Last().PhoneNumber,
            PhoneNumbersString = string.Join(", ", s.Select(p => p.PhoneNumber).Distinct()),
            EmailAddress = s.Last().EmailAddress,
            EmailAddressesString = string.Join(", ", s.Select(p => p.EmailAddress).Distinct()),
            IdentityNumber = s.Last().IdentityNumber,
            IdentityNumbersString = string.Join(", ", s.Select(p => p.IdentityNumber).Distinct()),
        })
        .OrderByDescending(o => o.AppUserId)
        .Select(s => s);
    }

    #endregion
}

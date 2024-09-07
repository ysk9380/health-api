using Microsoft.EntityFrameworkCore;

namespace Health.Api.Data;

public class HASDbContext : DbContext
{
    public HASDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AddressType> AddressTypes => Set<AddressType>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppUserAddress> AppUserAddresses => Set<AppUserAddress>();
    public DbSet<AppUserEmail> AppUserEmails => Set<AppUserEmail>();
    public DbSet<AppUserIdentity> AppUserIdentities => Set<AppUserIdentity>();
    public DbSet<AppUserPhone> AppUserPhones => Set<AppUserPhone>();
    public DbSet<AppUserRole> AppUserRoles => Set<AppUserRole>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<CustomerAppUser> CustomerAppUsers => Set<CustomerAppUser>();
    public DbSet<DataAuditHistory> DataAuditHistories => Set<DataAuditHistory>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Gender> Genders => Set<Gender>();
    public DbSet<IdentityType> IdentityTypes => Set<IdentityType>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Market> Markets => Set<Market>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientAddress> PatientAddresses => Set<PatientAddress>();
    public DbSet<PatientEmail> PatientEmails => Set<PatientEmail>();
    public DbSet<PatientIdentity> PatientIdentities => Set<PatientIdentity>();
    public DbSet<PatientPhone> PatientPhones => Set<PatientPhone>();
    public DbSet<PhoneType> PhoneTypes => Set<PhoneType>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<State> States => Set<State>();
    public DbSet<HealthVital> HealthVitals => Set<HealthVital>();
    public DbSet<PatientHealthVital> PatientHealthVitals => Set<PatientHealthVital>();
}
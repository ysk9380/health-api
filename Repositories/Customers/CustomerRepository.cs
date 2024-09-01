using Health.Api.Data;
using Health.Api.Models.Requests.Customer;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Customers;

public interface ICustomerRepository
{
    Task<Customer?> GetCustomerByShortNameAsync(string customerShortName);
    Task<Customer?> GetOtherCustomerWithSameShortNameAsync(string customerShortName, long customerId);
    Task<Customer?> GetCustomerByIdAsync(long customerId);
    Task<Customer?> GetCustomerByCodeAsync(string customerCode);
    Task<Customer> InsertCustomerAsync(InsertCustomerRequest newCustomerRequest);
    Task<Customer> UpdateCustomerAsync(UpdateCustomerRequest request);
    Task<IList<Customer>> GetLatestCustomersAsync();
    Task<int> GetCustomerCountAsync();
}

public class CustomerRepository : ICustomerRepository
{
    private HASDbContext _Context;
    private IDataAuditHistoryRepository _DataAuditHistoryRepository;

    public CustomerRepository(HASDbContext context
        , IDataAuditHistoryRepository dataAuditHistoryRepository)
    {
        _Context = context;
        _DataAuditHistoryRepository = dataAuditHistoryRepository;
    }

    public async Task<Customer?> GetCustomerByShortNameAsync(string customerShortName)
    {
        return await _Context.Customers
                        .FirstOrDefaultAsync(c => c.CustomerShortName.Equals(customerShortName));
    }

    public async Task<Customer?> GetOtherCustomerWithSameShortNameAsync(string customerShortName, long customerId)
    {
        return await _Context.Customers
                        .FirstOrDefaultAsync(p => p.CustomerShortName.Equals(customerShortName)
                            && !p.CustomerId.Equals(customerId));
    }

    public async Task<Customer?> GetCustomerByIdAsync(long customerId)
    {
        return await _Context.Customers
                    .Where(p => p.CustomerId.Equals(customerId))
                    .FirstOrDefaultAsync();
    }

    public async Task<Customer?> GetCustomerByCodeAsync(string customerCode)
    {
        return await _Context.Customers
                    .Where(p => p.CustomerCode.Equals(customerCode))
                    .FirstOrDefaultAsync();
    }

    public async Task<Customer> InsertCustomerAsync(InsertCustomerRequest request)
    {
        var newCustomer = new Customer
        {
            CustomerCode = Guid.NewGuid().ToString(),
            CustomerName = request.CustomerName,
            CustomerShortName = request.CustomerShortName,
        };

        _Context.Customers.Add(newCustomer);
        await _Context.SaveChangesAsync();

        newCustomer.CustomerCode = $"{DateTime.Now.ToString("yyyyMMdd")}{newCustomer.CustomerId}";
        _Context.Customers.Update(newCustomer);
        await _Context.SaveChangesAsync();

        return newCustomer;
    }

    public async Task<Customer> UpdateCustomerAsync(UpdateCustomerRequest request)
    {
        Customer? existingRecord = await GetCustomerByIdAsync(request.CustomerId);
        if (existingRecord == null)
            throw new FileNotFoundException($"Customer id ({request.CustomerId}) not found.");

        existingRecord.CustomerName = request.CustomerName;
        existingRecord.CustomerShortName = request.CustomerShortName;

        _Context.Customers.Update(existingRecord);
        await _Context.SaveChangesAsync();
        return existingRecord;
    }

    public async Task<IList<Customer>> GetLatestCustomersAsync()
    {
        return await _Context.Customers
                        .OrderByDescending(o => o.CustomerId)
                        .Take(100)
                        .ToListAsync();
    }

    public async Task<int> GetCustomerCountAsync()
    {
        return await _Context.Customers.CountAsync();
    }
}
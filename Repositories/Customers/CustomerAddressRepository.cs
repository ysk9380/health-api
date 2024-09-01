using Health.Api.Data;
using Health.Api.Models.Customer;
using Health.Api.Models.Requests.Customer;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Customers;

public interface ICustomerAddressRepository
{
    Task<CustomerAddress?> GetCustomerAddressByAddressTypeAsync(long customerId, byte addressTypeId);
    Task<CustomerAddress?> GetCustomerAddressByIdAsync(long customerAddressId);
    Task<CustomerAddressModel?> GetCustomerAddressModelByIdAsync(long customerAddressId);
    Task<IList<CustomerAddressModel>> GetCustomerAddressModelsByCustomerIdAsync(long customerId);
    Task<IList<CustomerAddress>> GetCustomerAddressesByCustomerIdAsync(long customerId);
    Task<CustomerAddress> InsertNewCustomerAddressAsync(InsertCustomerAddressRequest request, byte addressTypeId, byte stateId);
    Task<CustomerAddress> UpdateCustomerAddressRecordAsync(UpdateCustomerAddressRequest request, byte addressTypeId, byte stateId);
    Task DeactivateCustomerAddressAsync(long id);
}

public class CustomerAddressRepository : ICustomerAddressRepository
{
    private HASDbContext _Context;

    public CustomerAddressRepository(HASDbContext context)
    {
        _Context = context;
    }

    public async Task<CustomerAddress?> GetCustomerAddressByAddressTypeAsync(long customerId, byte addressTypeId)
    {
        CustomerAddress? address = await _Context.CustomerAddresses
                                    .FirstOrDefaultAsync(p => p.CustomerId.Equals(customerId)
                                        && p.AddressTypeId.Equals(addressTypeId)
                                        && p.IsActive);
        return address;
    }

    public async Task<CustomerAddress?> GetCustomerAddressByIdAsync(long customerAddressId)
    {
        return await _Context.CustomerAddresses
                        .Where(p => p.CustomerAddressId.Equals(customerAddressId))
                        .FirstOrDefaultAsync();
    }

    public async Task<CustomerAddressModel?> GetCustomerAddressModelByIdAsync(long customerAddressId)
    {
        return await (from pa in _Context.CustomerAddresses
                      join at in _Context.AddressTypes on pa.AddressTypeId equals at.AddressTypeId
                      join s in _Context.States on pa.StateId equals s.StateId
                      where pa.CustomerAddressId.Equals(customerAddressId) && pa.IsActive
                      select new CustomerAddressModel
                      {
                          CustomerAddressId = pa.CustomerAddressId,
                          CustomerId = pa.CustomerId,
                          AddressLine1 = pa.AddressLine1,
                          AddressLine2 = pa.AddressLine2,
                          AddressLine3 = pa.AddressLine3,
                          City = pa.City,
                          Pincode = pa.Pincode,
                          StateCode = s.StateCode,
                          StateName = s.StateName,
                          AddressTypeCode = at.AddressTypeCode,
                          AddressTypeName = at.AddressTypeName,
                      }).FirstOrDefaultAsync();
    }

    public async Task<IList<CustomerAddressModel>> GetCustomerAddressModelsByCustomerIdAsync(long customerId)
    {
        return await (from pa in _Context.CustomerAddresses
                      join at in _Context.AddressTypes on pa.AddressTypeId equals at.AddressTypeId
                      join s in _Context.States on pa.StateId equals s.StateId
                      where pa.CustomerId.Equals(customerId) && pa.IsActive
                      select new CustomerAddressModel
                      {
                          CustomerAddressId = pa.CustomerAddressId,
                          CustomerId = pa.CustomerId,
                          AddressLine1 = pa.AddressLine1,
                          AddressLine2 = pa.AddressLine2,
                          AddressLine3 = pa.AddressLine3,
                          City = pa.City,
                          Pincode = pa.Pincode,
                          StateCode = s.StateCode,
                          StateName = s.StateName,
                          AddressTypeCode = at.AddressTypeCode,
                          AddressTypeName = at.AddressTypeName,
                      }).ToListAsync();
    }

    public async Task<IList<CustomerAddress>> GetCustomerAddressesByCustomerIdAsync(long customerId)
    {
        return await _Context.CustomerAddresses
                        .Where(p => p.CustomerId.Equals(customerId) && p.IsActive)
                        .ToListAsync();
    }

    public async Task<CustomerAddress> InsertNewCustomerAddressAsync(InsertCustomerAddressRequest request
        , byte addressTypeId
        , byte stateId)
    {
        var newCustomerAddress = new CustomerAddress
        {
            CustomerId = request.CustomerId,
            AddressTypeId = addressTypeId,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            AddressLine3 = request.AddressLine3,
            City = request.City,
            Pincode = request.Pincode,
            StateId = stateId,
            IsActive = true
        };

        _Context.CustomerAddresses.Add(newCustomerAddress);
        await _Context.SaveChangesAsync();
        return newCustomerAddress;
    }

    public async Task<CustomerAddress> UpdateCustomerAddressRecordAsync(UpdateCustomerAddressRequest request, byte addressTypeId, byte stateId)
    {
        CustomerAddress? existingRecord = await GetCustomerAddressByIdAsync(request.CustomerAddressId);
        if (existingRecord == null)
            throw new FileNotFoundException($"Customer address id ({request.CustomerAddressId}) not found.");

        existingRecord.AddressTypeId = addressTypeId;
        existingRecord.AddressLine1 = request.AddressLine1;
        existingRecord.AddressLine2 = request.AddressLine2;
        existingRecord.AddressLine3 = request.AddressLine3;
        existingRecord.City = request.City;
        existingRecord.Pincode = request.Pincode;
        existingRecord.StateId = stateId;

        _Context.CustomerAddresses.Update(existingRecord);
        await _Context.SaveChangesAsync();

        return existingRecord;
    }

    public async Task DeactivateCustomerAddressAsync(long id)
    {
        CustomerAddress? existingRecord = await GetCustomerAddressByIdAsync(id);
        if (existingRecord == null)
            throw new FileNotFoundException($"Customer address id ({id}) not found.");

        existingRecord.IsActive = false;

        _Context.CustomerAddresses.Update(existingRecord);
        await _Context.SaveChangesAsync();
    }
}
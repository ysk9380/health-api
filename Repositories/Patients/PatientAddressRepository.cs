using Health.Api.Data;
using Health.Api.Models.Patient;
using Health.Api.Models.Requests.Patient;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Patients;

public interface IPatientAddressRepository
{
    Task<PatientAddress?> GetPatientAddressByAddressTypeAsync(long patientId, byte addressTypeId);
    Task<PatientAddress?> GetPatientAddressByIdAsync(long patientAddressId);
    Task<PatientAddressModel?> GetPatientAddressModelByIdAsync(long patientAddressId);
    Task<IList<PatientAddressModel>> GetPatientAddressModelsByPatientIdAsync(long patientId);
    Task<IList<PatientAddress>> GetPatientAddressesByPatientIdAsync(long patientId);
    Task<PatientAddress> InsertNewPatientAddressAsync(InsertPatientAddressRequest request, byte addressTypeId, byte stateId);
    Task<PatientAddress> UpdatePatientAddressRecordAsync(UpdatePatientAddressRequest request, byte addressTypeId, byte stateId);
    Task DeactivatePatientAddressAsync(long id);
}

public class PatientAddressRepository : IPatientAddressRepository
{
    private HASDbContext _Context;

    public PatientAddressRepository(HASDbContext context)
    {
        _Context = context;
    }

    public async Task<PatientAddress?> GetPatientAddressByAddressTypeAsync(long patientId, byte addressTypeId)
    {
        PatientAddress? address = await _Context.PatientAddresses
                                    .FirstOrDefaultAsync(p => p.PatientId.Equals(patientId)
                                        && p.AddressTypeId.Equals(addressTypeId)
                                        && p.IsActive);
        return address;
    }

    public async Task<PatientAddress?> GetPatientAddressByIdAsync(long patientAddressId)
    {
        return await _Context.PatientAddresses
                        .Where(p => p.PatientAddressId.Equals(patientAddressId))
                        .FirstOrDefaultAsync();
    }

    public async Task<PatientAddressModel?> GetPatientAddressModelByIdAsync(long patientAddressId)
    {
        return await (from pa in _Context.PatientAddresses
                      join at in _Context.AddressTypes on pa.AddressTypeId equals at.AddressTypeId
                      join s in _Context.States on pa.StateId equals s.StateId
                      where pa.PatientAddressId.Equals(patientAddressId) && pa.IsActive
                      select new PatientAddressModel
                      {
                          PatientAddressId = pa.PatientAddressId,
                          PatientId = pa.PatientId,
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

    public async Task<IList<PatientAddressModel>> GetPatientAddressModelsByPatientIdAsync(long patientId)
    {
        return await (from pa in _Context.PatientAddresses
                      join at in _Context.AddressTypes on pa.AddressTypeId equals at.AddressTypeId
                      join s in _Context.States on pa.StateId equals s.StateId
                      where pa.PatientId.Equals(patientId) && pa.IsActive
                      select new PatientAddressModel
                      {
                          PatientAddressId = pa.PatientAddressId,
                          PatientId = pa.PatientId,
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

    public async Task<IList<PatientAddress>> GetPatientAddressesByPatientIdAsync(long patientId)
    {
        return await _Context.PatientAddresses
                        .Where(p => p.PatientId.Equals(patientId) && p.IsActive)
                        .ToListAsync();
    }

    public async Task<PatientAddress> InsertNewPatientAddressAsync(InsertPatientAddressRequest request
        , byte addressTypeId
        , byte stateId)
    {
        var newPatientAddress = new PatientAddress
        {
            PatientId = request.PatientId,
            AddressTypeId = addressTypeId,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            AddressLine3 = request.AddressLine3,
            City = request.City,
            Pincode = request.Pincode,
            StateId = stateId,
            IsActive = true
        };

        _Context.PatientAddresses.Add(newPatientAddress);
        await _Context.SaveChangesAsync();
        return newPatientAddress;
    }

    public async Task<PatientAddress> UpdatePatientAddressRecordAsync(UpdatePatientAddressRequest request, byte addressTypeId, byte stateId)
    {
        PatientAddress? existingRecord = await GetPatientAddressByIdAsync(request.PatientAddressId);
        if (existingRecord == null)
            throw new FileNotFoundException($"Patient address id ({request.PatientAddressId}) not found.");

        existingRecord.AddressTypeId = addressTypeId;
        existingRecord.AddressLine1 = request.AddressLine1;
        existingRecord.AddressLine2 = request.AddressLine2;
        existingRecord.AddressLine3 = request.AddressLine3;
        existingRecord.City = request.City;
        existingRecord.Pincode = request.Pincode;
        existingRecord.StateId = stateId;

        _Context.PatientAddresses.Update(existingRecord);
        await _Context.SaveChangesAsync();

        return existingRecord;
    }

    public async Task DeactivatePatientAddressAsync(long id)
    {
        PatientAddress? existingRecord = await GetPatientAddressByIdAsync(id);
        if (existingRecord == null)
            throw new FileNotFoundException($"Patient address id ({id}) not found.");

        existingRecord.IsActive = false;

        _Context.PatientAddresses.Update(existingRecord);
        await _Context.SaveChangesAsync();
    }
}
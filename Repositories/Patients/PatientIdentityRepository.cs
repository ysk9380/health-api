using Health.Api.Data;
using Health.Api.Models.Patient;
using Health.Api.Models.Requests.Patient;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Patients;

public interface IPatientIdentityRepository
{
    Task<PatientIdentity?> GetPatientIdentityByIdentityTypeAndNumberAsync(long patientId, byte identityTypeId, string identityNumber);
    Task<PatientIdentity?> GetPatientIdentityByIdAsync(long patientIdentityId);
    Task<PatientIdentityModel?> GetPatientIdentityModelByIdAsync(long patientIdentityId);
    Task<IList<PatientIdentityModel>> GetPatientIdentityModelsByPatientIdAsync(long patientId);
    Task<IList<PatientIdentity>> GetPatientIdentitiesByPatientIdAsync(long patientId);
    Task<PatientIdentity> InsertNewPatientIdentityAsync(InsertPatientIdentityRequest request, byte identityTypeId);
    Task<PatientIdentity> UpdatePatientIdentityAsync(UpdatePatientIdentityRequest request, byte identityTypeId);
    Task DeactivatePatientIdentityAsync(long id);
}

public class PatientIdentityRepository : IPatientIdentityRepository
{
    private HASDbContext _Context;

    public PatientIdentityRepository(HASDbContext context)
    {
        _Context = context;
    }

    public async Task<PatientIdentity?> GetPatientIdentityByIdentityTypeAndNumberAsync(long patientId, byte identityTypeId, string identityNumber)
    {
        return await _Context.PatientIdentities
                        .Where(p => p.PatientId.Equals(patientId)
                            && p.IdentityTypeId.Equals(identityTypeId)
                            && p.IdentityNumber.Equals(identityNumber.Trim())
                            && p.IsActive)
                        .FirstOrDefaultAsync();
    }

    public async Task<PatientIdentity?> GetPatientIdentityByIdAsync(long patientIdentityId)
    {
        return await _Context.PatientIdentities
                        .Where(p => p.PatientIdentityId.Equals(patientIdentityId)
                            && p.IsActive)
                        .FirstOrDefaultAsync();
    }

    public async Task<PatientIdentityModel?> GetPatientIdentityModelByIdAsync(long patientIdentityId)
    {
        return await (from pi in _Context.PatientIdentities
                      join it in _Context.IdentityTypes on pi.IdentityTypeId equals it.IdentityTypeId
                      where pi.PatientIdentityId.Equals(patientIdentityId) && pi.IsActive
                      select new PatientIdentityModel
                      {
                          PatientIdentityId = pi.PatientIdentityId,
                          PatientId = pi.PatientId,
                          IdentityNumber = pi.IdentityNumber,
                          IssuedBy = pi.IssuedBy,
                          PlaceIssued = pi.PlaceIssued,
                          Expiry = pi.Expiry,
                          IdentityTypeCode = it.IdentityTypeCode,
                          IdentityTypeName = it.IdentityTypeName
                      }).FirstOrDefaultAsync();
    }

    public async Task<IList<PatientIdentityModel>> GetPatientIdentityModelsByPatientIdAsync(long patientId)
    {
        return await (from pi in _Context.PatientIdentities
                      join it in _Context.IdentityTypes on pi.IdentityTypeId equals it.IdentityTypeId
                      where pi.PatientId.Equals(patientId) && pi.IsActive
                      select new PatientIdentityModel
                      {
                          PatientIdentityId = pi.PatientIdentityId,
                          PatientId = pi.PatientId,
                          IdentityNumber = pi.IdentityNumber,
                          IssuedBy = pi.IssuedBy,
                          PlaceIssued = pi.PlaceIssued,
                          Expiry = pi.Expiry,
                          IdentityTypeCode = it.IdentityTypeCode,
                          IdentityTypeName = it.IdentityTypeName
                      }).ToListAsync();
    }

    public async Task<IList<PatientIdentity>> GetPatientIdentitiesByPatientIdAsync(long patientId)
    {
        return await _Context.PatientIdentities
                        .Where(p => p.PatientId.Equals(patientId) && p.IsActive)
                        .ToListAsync();
    }

    public async Task<PatientIdentity> InsertNewPatientIdentityAsync(InsertPatientIdentityRequest request, byte identityTypeId)
    {
        var newPatientIdentity = new PatientIdentity
        {
            PatientId = request.PatientId,
            IdentityTypeId = identityTypeId,
            IdentityNumber = request.IdentityNumber,
            IssuedBy = request.IssuedBy,
            PlaceIssued = request.PlaceIssued,
            Expiry = request.Expiry,
            IsActive = true
        };

        _Context.PatientIdentities.Add(newPatientIdentity);
        await _Context.SaveChangesAsync();
        return newPatientIdentity;
    }

    public async Task<PatientIdentity> UpdatePatientIdentityAsync(UpdatePatientIdentityRequest request, byte identityTypeId)
    {
        PatientIdentity? existingRecord = await GetPatientIdentityByIdAsync(request.PatientIdentityId);
        if (existingRecord == null)
            throw new FileNotFoundException($"Patient identity id ({request.PatientIdentityId}) not found.");

        existingRecord.IdentityTypeId = identityTypeId;
        existingRecord.IdentityNumber = request.IdentityNumber;
        existingRecord.IssuedBy = request.IssuedBy;
        existingRecord.PlaceIssued = request.PlaceIssued;
        existingRecord.Expiry = request.Expiry;

        _Context.Update(existingRecord);
        await _Context.SaveChangesAsync();
        return existingRecord;
    }

    public async Task DeactivatePatientIdentityAsync(long id)
    {
        PatientIdentity? existingRecord = await GetPatientIdentityByIdAsync(id);
        if (existingRecord == null)
            throw new FileNotFoundException($"Patient identity id ({id}) not found.");

        existingRecord.IsActive = false;

        _Context.PatientIdentities.Update(existingRecord);
        await _Context.SaveChangesAsync();
    }
}
using Health.Api.Data;
using Health.Api.Models.Requests.Patient;
using Microsoft.EntityFrameworkCore;
using Health.Api.Models.Patient;

namespace Health.Api.Repositories.Patients;

public interface IPatientPhoneRepository
{
    Task<PatientPhone?> GetPatientPhoneByPhoneNumberAsync(long patientId, string phoneNumber);
    Task<PatientPhone?> GetPatientPhoneByIdAsync(long patientPhoneId);
    Task<IList<PatientPhoneModel>> GetPatientPhoneModelsByPatientIdAsync(long patientId);
    Task<PatientPhoneModel?> GetPatientPhoneModelByIdAsync(long patientPhoneId);
    Task<IList<PatientPhone>> GetPatientPhonesByPatientIdAsync(long patientId);
    Task<PatientPhone> InsertNewPatientPhoneAsync(InsertPatientPhoneRequest request, byte phoneTypeId);
    Task<PatientPhone> UpdatePatientPhoneAsync(UpdatePatientPhoneRequest request, byte phoneTypeId);
    Task DeactivatePatientPhoneAsync(long id);
}

public class PatientPhoneRepository : IPatientPhoneRepository
{
    private HASDbContext _Context;

    public PatientPhoneRepository(HASDbContext context)
    {
        _Context = context;
    }

    public async Task<PatientPhone?> GetPatientPhoneByPhoneNumberAsync(long patientId, string phoneNumber)
    {
        return await _Context.PatientPhones
                        .Where(p => p.PatientId.Equals(patientId)
                                && p.PhoneNumber.Equals(phoneNumber.Trim())
                                && p.IsActive)
                        .OrderByDescending(o => o.PatientPhoneId)
                        .FirstOrDefaultAsync();
    }

    public async Task<PatientPhone?> GetPatientPhoneByIdAsync(long patientPhoneId)
    {
        return await _Context.PatientPhones
                        .Where(p => p.PatientPhoneId.Equals(patientPhoneId))
                        .FirstOrDefaultAsync();
    }

    public async Task<IList<PatientPhoneModel>> GetPatientPhoneModelsByPatientIdAsync(long patientId)
    {
        return await (from pp in _Context.PatientPhones
                      join pt in _Context.PhoneTypes on pp.PhoneTypeId equals pt.PhoneTypeId
                      where pp.PatientId.Equals(patientId) && pp.IsActive
                      select new PatientPhoneModel
                      {
                          PatientPhoneId = pp.PatientPhoneId,
                          PatientId = pp.PatientId,
                          PhoneNumber = pp.PhoneNumber,
                          ListedAs = pp.ListedAs,
                          PhoneTypeCode = pt.PhoneTypeCode,
                          PhoneTypeName = pt.PhoneTypeName
                      }).ToListAsync();
    }

    public async Task<PatientPhoneModel?> GetPatientPhoneModelByIdAsync(long patientPhoneId)
    {
        return await (from pp in _Context.PatientPhones
                      join pt in _Context.PhoneTypes on pp.PhoneTypeId equals pt.PhoneTypeId
                      where pp.PatientPhoneId.Equals(patientPhoneId) && pp.IsActive
                      select new PatientPhoneModel
                      {
                          PatientPhoneId = pp.PatientPhoneId,
                          PatientId = pp.PatientId,
                          PhoneNumber = pp.PhoneNumber,
                          ListedAs = pp.ListedAs,
                          PhoneTypeCode = pt.PhoneTypeCode,
                          PhoneTypeName = pt.PhoneTypeName
                      }).FirstOrDefaultAsync();
    }

    public async Task<IList<PatientPhone>> GetPatientPhonesByPatientIdAsync(long patientId)
    {
        return await _Context.PatientPhones
                        .Where(p => p.PatientId.Equals(patientId) && p.IsActive)
                        .ToListAsync();
    }

    public async Task<PatientPhone> InsertNewPatientPhoneAsync(InsertPatientPhoneRequest request
        , byte phoneTypeId)
    {
        var newPatientPhone = new PatientPhone
        {
            PatientId = request.PatientId,
            PhoneTypeId = phoneTypeId,
            PhoneNumber = request.PhoneNumber.Trim(),
            ListedAs = request.ListedAs,
            IsActive = true
        };

        _Context.PatientPhones.Add(newPatientPhone);
        await _Context.SaveChangesAsync();
        return newPatientPhone;
    }

    public async Task<PatientPhone> UpdatePatientPhoneAsync(UpdatePatientPhoneRequest request
        , byte phoneTypeId)
    {
        PatientPhone? existingRecord = await GetPatientPhoneByIdAsync(request.PatientPhoneId);
        if (existingRecord == null)
            throw new FileNotFoundException($"Patient phone id ({request.PatientPhoneId}) not found.");

        existingRecord.PhoneNumber = request.PhoneNumber;
        existingRecord.PhoneTypeId = phoneTypeId;
        existingRecord.ListedAs = request.ListedAs;

        _Context.Update(existingRecord);
        await _Context.SaveChangesAsync();
        return existingRecord;
    }

    public async Task DeactivatePatientPhoneAsync(long id)
    {
        PatientPhone? existingRecord = await GetPatientPhoneByIdAsync(id);
        if (existingRecord == null)
            throw new FileNotFoundException($"Patient phone id ({id}) not found.");

        existingRecord.IsActive = false;

        _Context.PatientPhones.Update(existingRecord);
        await _Context.SaveChangesAsync();
    }
}
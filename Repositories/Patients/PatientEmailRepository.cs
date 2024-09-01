using Health.Api.Data;
using Health.Api.Models.Requests.Patient;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Patients;

public interface IPatientEmailRepository
{
    Task<PatientEmail?> GetPatientEmailByEmailAddressAsync(long patientId, string emailAddress);
    Task<PatientEmail?> GetPatientEmailByIdAsync(long patientEmailId);
    Task<IList<PatientEmail>> GetPatientEmailsByPatientIdAsync(long patientId);
    Task<PatientEmail> InsertNewPatientEmailAsync(InsertPatientEmailRequest request);
    Task<PatientEmail> UpdatePatientEmailAsync(UpdatePatientEmailRequest request);
    Task DeactivatePatientEmailAsync(long id);
}

public class PatientEmailRepository : IPatientEmailRepository
{
    private HASDbContext _Context;

    public PatientEmailRepository(HASDbContext context)
    {
        _Context = context;
    }

    public async Task<PatientEmail?> GetPatientEmailByEmailAddressAsync(long patientId, string emailAddress)
    {
        return await _Context.PatientEmails
                        .Where(p => p.PatientId.Equals(patientId)
                            && p.EmailAddress.Equals(emailAddress.Trim())
                            && p.IsActive)
                        .FirstOrDefaultAsync();
    }

    public async Task<PatientEmail?> GetPatientEmailByIdAsync(long patientEmailId)
    {
        return await _Context.PatientEmails
                        .Where(p => p.PatientEmailId.Equals(patientEmailId)
                            && p.IsActive)
                        .FirstOrDefaultAsync();
    }

    public async Task<IList<PatientEmail>> GetPatientEmailsByPatientIdAsync(long patientId)
    {
        return await _Context.PatientEmails
                        .Where(p => p.PatientId.Equals(patientId) && p.IsActive)
                        .ToListAsync();
    }

    public async Task<PatientEmail> InsertNewPatientEmailAsync(InsertPatientEmailRequest request)
    {
        var newPatientEmail = new PatientEmail
        {
            PatientId = request.PatientId,
            EmailAddress = request.EmailAddress,
            IsActive = true
        };

        _Context.PatientEmails.Add(newPatientEmail);
        await _Context.SaveChangesAsync();
        return newPatientEmail;
    }

    public async Task<PatientEmail> UpdatePatientEmailAsync(UpdatePatientEmailRequest request)
    {
        PatientEmail? existingRecord = await GetPatientEmailByIdAsync(request.PatientEmailId);
        if (existingRecord == null)
            throw new FileNotFoundException($"Patient email id ({request.PatientEmailId}) not found.");

        existingRecord.EmailAddress = request.EmailAddress;

        _Context.Update(existingRecord);
        await _Context.SaveChangesAsync();
        return existingRecord;
    }

    public async Task DeactivatePatientEmailAsync(long id)
    {
        PatientEmail? existingRecord = await GetPatientEmailByIdAsync(id);
        if (existingRecord == null)
            throw new FileNotFoundException($"Patient email id ({id}) not found.");

        existingRecord.IsActive = false;

        _Context.PatientEmails.Update(existingRecord);
        await _Context.SaveChangesAsync();
    }
}
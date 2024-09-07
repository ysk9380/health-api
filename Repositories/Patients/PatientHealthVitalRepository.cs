using Health.Api.Data;
using Health.Api.Models.Requests.Patient;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Patients;

public interface IPatientHealthVitalRepository
{
    Task<PatientHealthVital?> GetPatientHealthVitalById(long patientHealthVitalId);
    Task<List<PatientHealthVital>> GetPatientHealthVitalsAsync(long patientId);
    Task<PatientHealthVital> InsertNewPatientHealthVitalAsync(InsertPatientHealthVitalRequest request, int healthVitalId);
    Task DeactivatePatientEmailAsync(long id);
}

public class PatientHealthVitalRepository : IPatientHealthVitalRepository
{
    private readonly HASDbContext _dbContext;

    public PatientHealthVitalRepository(HASDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PatientHealthVital?> GetPatientHealthVitalById(long patientHealthVitalId)
    {
        return await _dbContext.PatientHealthVitals
            .Where(x => x.PatientHealthVitalId.Equals(patientHealthVitalId))
            .FirstOrDefaultAsync();
    }

    public async Task<List<PatientHealthVital>> GetPatientHealthVitalsAsync(long patientId)
    {
        return await _dbContext.PatientHealthVitals
            .Where(x => x.PatientId.Equals(patientId))
            .ToListAsync();
    }

    public async Task<PatientHealthVital> InsertNewPatientHealthVitalAsync(InsertPatientHealthVitalRequest request, int healthVitalId)
    {
        var newPatientHealthVital = new PatientHealthVital
        {
            PatientId = request.PatientId,
            HealthVitalId = healthVitalId,
            Content = request.Content,
            CreatedTimestamp = DateTime.Now,
            IsActive = true
        };

        _dbContext.PatientHealthVitals.Add(newPatientHealthVital);
        await _dbContext.SaveChangesAsync();
        return newPatientHealthVital;
    }

    public async Task DeactivatePatientEmailAsync(long id)
    {
        var patientHealthVital = await _dbContext.PatientHealthVitals
            .Where(x => x.PatientHealthVitalId.Equals(id))
            .FirstOrDefaultAsync();

        if (patientHealthVital != null)
        {
            patientHealthVital.IsActive = false;
            await _dbContext.SaveChangesAsync();
        }
    }
}
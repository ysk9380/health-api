using Health.Api.Data;
using Health.Api.Models.Patient;
using Health.Api.Models.Requests.Patient;
using Health.Api.Models.Standard;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Patients;

public interface IPatientRepository
{
    Task<Patient?> GetPatientByNameAndDoBAsync(string firstname, string lastname, DateTime dateOfBirth);
    Task<Patient?> GetOtherPatientWithSameNameAndDoBAsync(string firstname, string lastname, DateTime dateOfBirth, long patientId);
    Task<PatientProfile?> GetPatientProfileByIdAsync(long patientId);
    Task<Patient?> GetPatientByIdAsync(long patientId);
    Task<PatientMinimalDetail?> GetPatientByCodeAsync(string patientCode);
    Task<IList<PatientMinimalDetail>> SearchPatientsAsync(PatientSearchRequest request);
    Task<Patient> InsertNewPatientRecordAsync(InsertPatientRequest newPatientRequest, byte genderId);
    Task<Patient> UpdatePatientRecordAsync(UpdatePatientRequest request, byte genderId);
}

public class PatientRepository : IPatientRepository
{
    private HASDbContext _Context;
    private IDataAuditHistoryRepository _DataAuditHistoryRepository;

    private IEnumerable<PatientMinimalDetail> GetGroupedPatientRecords(IList<PatientMinimalDetail> patientRecords)
    {
        return patientRecords.GroupBy(g => g.PatientId).Select(s => new PatientMinimalDetail
        {
            PatientId = s.Key,
            PatientCode = s.First().PatientCode,
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
        .OrderByDescending(o => o.PatientId)
        .Select(s => s);
    }

    public PatientRepository(HASDbContext context, IDataAuditHistoryRepository dataAuditHistoryRepository)
    {
        _Context = context;
        _DataAuditHistoryRepository = dataAuditHistoryRepository;
    }

    public async Task<Patient?> GetPatientByNameAndDoBAsync(string firstname
        , string lastname
        , DateTime dateOfBirth)
    {
        return await _Context.Patients.FirstOrDefaultAsync(p => p.Firstname.Equals(firstname)
                        && p.Lastname.Equals(lastname)
                        && p.DateOfBirth.Equals(dateOfBirth));
    }

    public async Task<Patient?> GetOtherPatientWithSameNameAndDoBAsync(string firstname
        , string lastname
        , DateTime dateOfBirth
        , long patientId)
    {
        return await _Context.Patients.FirstOrDefaultAsync(p => p.Firstname.Equals(firstname)
                        && p.Lastname.Equals(lastname)
                        && p.DateOfBirth.Equals(dateOfBirth)
                        && !p.PatientId.Equals(patientId));
    }

    public async Task<PatientProfile?> GetPatientProfileByIdAsync(long patientId)
    {
        return await (from p in _Context.Patients
                      join g in _Context.Genders on p.GenderId equals g.GenderId
                      where p.PatientId.Equals(patientId)
                      select new PatientProfile
                      {
                          PatientId = p.PatientId,
                          PatientCode = p.PatientCode,
                          Firstname = p.Firstname,
                          Middlename = p.Middlename,
                          Lastname = p.Lastname,
                          DateOfBirth = p.DateOfBirth,
                          GenderCode = g.GenderCode,
                          GenderName = g.GenderName,
                      }).FirstOrDefaultAsync();
    }

    public async Task<Patient?> GetPatientByIdAsync(long patientId)
    {
        return await _Context.Patients
                    .Where(p => p.PatientId.Equals(patientId))
                    .FirstOrDefaultAsync();
    }

    public async Task<PatientMinimalDetail?> GetPatientByCodeAsync(string patientCode)
    {
        var patientPhonesQuery = _Context.PatientPhones.Where(ph => ph.IsActive);
        var patientEmailsQuery = _Context.PatientEmails.Where(pe => pe.IsActive);
        var patientIdentityQuery = _Context.PatientIdentities.Where(pi => pi.IsActive);

        var searchResult = await (from p in _Context.Patients
                                  join ph in patientPhonesQuery on p.PatientId equals ph.PatientId into patientPhoneGroup

                                  from ppg in patientPhoneGroup.DefaultIfEmpty()
                                  join pe in patientEmailsQuery on p.PatientId equals pe.PatientId into patientEmailGroup

                                  from peg in patientEmailGroup.DefaultIfEmpty()
                                  join pi in patientIdentityQuery on p.PatientId equals pi.PatientId into patientIdentityGroup

                                  from pig in patientIdentityGroup.DefaultIfEmpty()
                                  join g in _Context.Genders on p.GenderId equals g.GenderId
                                  where p.PatientCode.Equals(patientCode)
                                  select new PatientMinimalDetail
                                  {
                                      PatientId = p.PatientId,
                                      PatientCode = p.PatientCode,
                                      Firstname = p.Firstname,
                                      Middlename = p.Middlename,
                                      Lastname = p.Lastname,
                                      DateOfBirth = p.DateOfBirth,
                                      Gender = g.GenderName,
                                      PhoneNumber = ppg.PhoneNumber,
                                      EmailAddress = peg.EmailAddress,
                                      IdentityNumber = pig.IdentityNumber
                                  }).ToListAsync();

        PatientMinimalDetail? groupedSearchResult = GetGroupedPatientRecords(searchResult).FirstOrDefault();

        return groupedSearchResult;
    }

    public async Task<IList<PatientMinimalDetail>> SearchPatientsAsync(PatientSearchRequest request)
    {
        var patientsQuery = _Context.Patients.AsQueryable<Patient>();
        var patientPhonesQuery = _Context.PatientPhones.Where(ph => ph.IsActive).AsQueryable<PatientPhone>();
        var patientEmailsQuery = _Context.PatientEmails.Where(pe => pe.IsActive).AsQueryable<PatientEmail>();
        var patientIdentityQuery = _Context.PatientIdentities.Where(pi => pi.IsActive).AsQueryable<PatientIdentity>();

        var searchQuery = (from p in patientsQuery
                           join ph in patientPhonesQuery on p.PatientId equals ph.PatientId into patientPhoneGroup

                           from ppg in patientPhoneGroup.DefaultIfEmpty()
                           join pe in patientEmailsQuery on p.PatientId equals pe.PatientId into patientEmailGroup

                           from peg in patientEmailGroup.DefaultIfEmpty()
                           join pi in patientIdentityQuery on p.PatientId equals pi.PatientId into patientIdentityGroup

                           from pig in patientIdentityGroup.DefaultIfEmpty()
                           join g in _Context.Genders on p.GenderId equals g.GenderId
                           select new
                           {
                               PatientId = p.PatientId,
                               PatientCode = p.PatientCode,
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

        IList<PatientMinimalDetail> searchResult = await searchQuery.Select(s => new PatientMinimalDetail
        {
            PatientId = s.PatientId,
            PatientCode = s.PatientCode,
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

        IList<PatientMinimalDetail> groupedSearchResult = GetGroupedPatientRecords(searchResult)
        .ToList();

        return groupedSearchResult;
    }

    public async Task<Patient> InsertNewPatientRecordAsync(InsertPatientRequest request
        , byte genderId)
    {
        var newPatient = new Patient
        {
            Firstname = request.Firstname.Trim(),
            Middlename = request.Middlename?.Trim(),
            Lastname = request.Lastname.Trim(),
            DateOfBirth = request.DateOfBirth,
            PatientCode = Guid.NewGuid().ToString(),
            GenderId = genderId
        };

        _Context.Patients.Add(newPatient);
        await _Context.SaveChangesAsync();

        newPatient.PatientCode = $"{DateTime.Now.ToString("yyyyMMdd")}{newPatient.PatientId}";
        _Context.Patients.Update(newPatient);
        await _Context.SaveChangesAsync();

        return newPatient;
    }

    public async Task<Patient> UpdatePatientRecordAsync(UpdatePatientRequest request
        , byte genderId)
    {
        Patient? existingRecord = await GetPatientByIdAsync(request.PatientId);
        if (existingRecord == null)
            throw new FileNotFoundException($"Patient id ({request.PatientId}) not found.");

        existingRecord.Firstname = request.Firstname;
        existingRecord.Middlename = request.Middlename;
        existingRecord.Lastname = request.Lastname;
        existingRecord.DateOfBirth = request.DateOfBirth;
        existingRecord.GenderId = genderId;

        _Context.Patients.Update(existingRecord);
        await _Context.SaveChangesAsync();
        return existingRecord;
    }
}
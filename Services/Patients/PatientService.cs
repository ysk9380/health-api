using System.Security.Claims;
using FluentValidation;
using Health.Api.Authentication;
using Health.Api.Data;
using Health.Api.Models.Patient;
using Health.Api.Models.Requests.Patient;
using Health.Api.Models.Standard;
using Health.Api.Repositories;
using Health.Api.Repositories.Patients;

namespace Health.Api.Services.Patients;

public class PatientService
{
    private const string BaseService = "/api/patients";
    private const string PatientTableName = "patient";

    internal static async Task<IResult> InsertPatientAsync(InsertPatientRequest request
            , IValidator<InsertPatientRequest> requestValidator
            , IPatientRepository patientRepository
            , IMasterRepository masterRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        if (!request.ForceCreate)
        {
            var existingRecord
                = await patientRepository.GetPatientByNameAndDoBAsync(request.Firstname
                    , request.Lastname
                    , request.DateOfBirth);

            if (existingRecord != null)
                return Results.Conflict<Patient>(existingRecord);
        }

        byte genderId = await masterRepository.GetGenderIdAsync(request.GenderCode);
        if (genderId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_GENDER_CODE",
                Note = $"Gender code ({request.GenderCode}) is unrecognized."
            });

        Patient newPatient = await patientRepository.InsertNewPatientRecordAsync(request, genderId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordNewModelAuditDataAsync<Patient>(newPatient
                , PatientTableName
                , newPatient.PatientId
                , customerId
                , appUserId);

        return Results.Created($"{BaseService}/{newPatient.PatientId}", newPatient);
    }

    internal static async Task<IResult> UpdatePatientAsync(UpdatePatientRequest request
            , IValidator<UpdatePatientRequest> requestValidator
            , IPatientRepository patientRepository
            , IMasterRepository masterRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var matchingOtherRecord
            = await patientRepository.GetOtherPatientWithSameNameAndDoBAsync(request.Firstname
                , request.Lastname
                , request.DateOfBirth
                , request.PatientId);
        if (matchingOtherRecord != null)
            return Results.Conflict<Patient>(matchingOtherRecord);

        byte genderId = await masterRepository.GetGenderIdAsync(request.GenderCode);
        if (genderId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_GENDER_CODE",
                Note = $"Gender code ({request.GenderCode}) is unrecognized."
            });

        var existingRecord = await patientRepository.GetPatientByIdAsync(request.PatientId);
        if (existingRecord == null)
            return Results.BadRequest(new
            {
                Code = "PATIENT_RECORD_NOT_FOUND",
                Note = $"Patient with id ({request.PatientId}) does not exist."
            });
        var existingRecordCopy = new Patient(existingRecord);

        Patient updatedRecord = await patientRepository.UpdatePatientRecordAsync(request, genderId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordEditModelAuditDataAsync(existingRecordCopy
                , updatedRecord
                , PatientTableName
                , updatedRecord.PatientId
                , customerId
                , appUserId);
        return Results.Accepted($"{BaseService}/{updatedRecord.PatientId}", updatedRecord);
    }

    internal static async Task<IResult> GetPatientProfileByIdAsync(long id
            , IPatientRepository patientRepository)
    {
        PatientProfile? patientProfile = await patientRepository.GetPatientProfileByIdAsync(id);
        return Results.Ok(patientProfile);
    }

    internal static async Task<IResult> GetPatientDetailByCodeAsync(string code
            , IPatientRepository patientRepository)
    {
        PatientMinimalDetail? patientDetail = await patientRepository.GetPatientByCodeAsync(code);
        return Results.Ok(patientDetail);
    }

    internal static async Task<IResult> SearchPatientsAsync(PatientSearchRequest request
            , IPatientRepository patientRepository)
    {
        if (string.IsNullOrWhiteSpace(request.Firstname)
            && string.IsNullOrWhiteSpace(request.Lastname)
            && string.IsNullOrWhiteSpace(request.PhoneNumber)
            && string.IsNullOrWhiteSpace(request.Email)
            && string.IsNullOrWhiteSpace(request.IdentityNumber))
            return Results.NoContent();

        IList<PatientMinimalDetail> searchResult = await patientRepository.SearchPatientsAsync(request);

        return Results.Ok(searchResult);
    }

    public static void Register(WebApplication app)
    {
        app.MapPost(BaseService, InsertPatientAsync)
        .WithName("InsertPatient")
        .WithDescription("Inserts a new patient record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<Patient>(409)
        .Produces<Patient>(201)
        .RequireAuthorization();

        app.MapPut(BaseService, UpdatePatientAsync)
        .WithName("UpdatePatient")
        .WithDescription("Updates an existing patient record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<Patient>(409)
        .Produces<Patient>(202)
        .RequireAuthorization();

        app.MapGet(string.Join("/", BaseService, "{id}"), GetPatientProfileByIdAsync)
        .WithName("GetPatientById")
        .WithDescription("Get patient information using patient id.")
        .Produces<PatientProfile>(200)
        .Produces(401)
        .RequireAuthorization();

        app.MapGet(string.Join("/", BaseService, "code/{code}"), GetPatientDetailByCodeAsync)
        .WithName("GetPatientByPatientCode")
        .WithDescription("Get patient information using patient code.")
        .Produces<PatientMinimalDetail>(200)
        .Produces(401)
        .RequireAuthorization();

        app.MapPost(string.Join("/", BaseService, "search"), SearchPatientsAsync)
        .WithName("SearchPatients")
        .WithDescription("Fetches patient email records using patient id.")
        .Produces<PatientMinimalDetail[]>(200)
        .Produces(401)
        .RequireAuthorization();

        PatientAddressService.Register(app);
        PatientPhoneService.Register(app);
        PatientEmailService.Register(app);
        PatientIdentityService.Register(app);
    }
}
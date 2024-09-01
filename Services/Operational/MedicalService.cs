using FluentValidation;
using Health.Api.Data;
using Health.Api.Models.Constants;
using Health.Api.Models.Operational;
using Health.Api.Models.Requests.Operational;
using Health.Api.Repositories;
using Health.Api.Repositories.Operational;

namespace Health.Api.Services.Operational;

public class MedicalService
{
    private const string BaseService = "/api/operational/services";

    internal static async Task<IResult> GetServicesAsync(IMedicalServiceRepository medicalServiceRepository)
    {
        IList<ServiceModel> serviceModels = await medicalServiceRepository.GetServiceModelsAsync();
        return Results.Ok(serviceModels);
    }

    internal static async Task<IResult> GetServiceModelByIdAsync(byte id
        , IMedicalServiceRepository medicalServiceRepository)
    {
        ServiceModel? serviceModel = await medicalServiceRepository.GetServiceModelByIdAsync(id);
        return Results.Ok(serviceModel);
    }

    internal static async Task<IResult> GetServiceModelByCodeAsync(string code
        , IMedicalServiceRepository medicalServiceRepository)
    {
        ServiceModel? serviceModel = await medicalServiceRepository.GetServiceModelByCodeAsync(code);
        return Results.Ok(serviceModel);
    }

    internal static async Task<IResult> InsertServiceAsync(InsertServiceRequest request
        , IValidator<InsertServiceRequest> requestValidator
        , IMedicalServiceRepository medicalServiceRepository
        , IMasterRepository masterRepository
        , ILogger<MedicalService> logger)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        byte serviceCategoryId = await masterRepository.GetServiceCategoryIdAsync(request.ServiceCategoryCode);
        if (serviceCategoryId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_SERVICE_CATEGORY_CODE",
                Note = $"Service category code ({request.ServiceCategoryCode}) is unrecognized."
            });

        ServiceModel? existingRecord = await medicalServiceRepository.GetServiceModelByCodeAsync(request.ServiceCode);
        if (existingRecord != null)
        {
            logger.LogError("Service Insertion Conflict. Another service existing with same code {serviceCode}"
                , request.ServiceCode);
            return Results.Conflict<ServiceModel>(existingRecord);
        }

        Service newService = await medicalServiceRepository.InsertServiceAsync(request, serviceCategoryId);
        return Results.Created($"api/operational/medicalservices/{newService.ServiceId}", newService);
    }

    internal static async Task<IResult> UpdateServiceAsync(UpdateServiceRequest request
        , IValidator<UpdateServiceRequest> requestValidator
        , IMasterRepository masterRepository
        , IMedicalServiceRepository medicalServiceRepository
        , ILogger<MedicalService> logger)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        byte serviceCategoryId = await masterRepository.GetServiceCategoryIdAsync(request.ServiceCategoryCode);
        if (serviceCategoryId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_SERVICE_CATEGORY_CODE",
                Note = $"Service category code ({request.ServiceCategoryCode}) is unrecognized."
            });

        ServiceModel? existingRecord = await medicalServiceRepository.GetServiceModelByCodeAsync(request.ServiceCode);
        if (existingRecord != null && existingRecord.ServiceId != request.ServiceId)
        {
            logger.LogError("Service Update Conflict. Another service existing with same code {serviceCode}"
                , request.ServiceCode);
            return Results.Conflict<ServiceModel>(existingRecord);
        }

        Service updatedService = await medicalServiceRepository.UpdateServiceAsync(request, serviceCategoryId);
        return Results.Accepted($"api/operational/medicalservices/{updatedService.ServiceId}", updatedService);
    }

    public static void Register(WebApplication app)
    {
        app.MapGet(BaseService, GetServicesAsync)
        .WithName("GetServices")
        .WithDescription("Returns the list of services.")
        .Produces<IList<ServiceModel>>(200)
        .RequireAuthorization();

        app.MapGet(string.Join("", BaseService, "/{id}"), GetServiceModelByIdAsync)
        .WithName("GetServiceById")
        .WithDescription("Returns a service based on id.")
        .Produces<ServiceModel>(200)
        .RequireAuthorization();

        app.MapGet(string.Join("", BaseService, "/code/{code}"), GetServiceModelByCodeAsync)
        .WithName("GetServiceByCode")
        .WithDescription("Returns a service based on code.")
        .Produces<ServiceModel>(200)
        .RequireAuthorization();

        app.MapPost(BaseService, InsertServiceAsync)
        .WithName("InsertService")
        .WithDescription("Inserts a new service.")
        .Produces<ServiceModel>(409)
        .Produces<Service>(200)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPut(BaseService, UpdateServiceAsync)
        .WithName("UpdateService")
        .WithDescription("Updates an existing service.")
        .Produces<ServiceModel>(409)
        .Produces<Service>(200)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });
    }
}
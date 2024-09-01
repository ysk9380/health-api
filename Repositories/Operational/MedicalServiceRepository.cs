using Health.Api.Data;
using Health.Api.Models.Operational;
using Health.Api.Models.Requests.Operational;
using Microsoft.EntityFrameworkCore;

namespace Health.Api.Repositories.Operational;

public interface IMedicalServiceRepository
{
    Task<IList<ServiceModel>> GetServiceModelsAsync();
    Task<ServiceModel?> GetServiceModelByIdAsync(int serviceId);
    Task<ServiceModel?> GetServiceModelByCodeAsync(string serviceCode);
    Task<Service?> GetServiceByIdAsync(int serviceId);
    Task<Service> InsertServiceAsync(InsertServiceRequest request, byte serviceCategoryId);
    Task<Service> UpdateServiceAsync(UpdateServiceRequest request, byte serviceCategoryId);
    Task<int> GetMedicalServicesCountAsync();
}

public class MedicalServiceRepository : IMedicalServiceRepository
{
    public HASDbContext _Context;

    public MedicalServiceRepository(HASDbContext context)
    {
        _Context = context;
    }

    public async Task<ServiceModel?> GetServiceModelByIdAsync(int serviceId)
    {
        ServiceModel? serviceModel = await (from s in _Context.Services
                                            join sc in _Context.ServiceCategories on s.ServiceCategoryId equals sc.ServiceCategoryId
                                            where s.ServiceId.Equals(serviceId)
                                            select new ServiceModel
                                            {
                                                ServiceId = s.ServiceId,
                                                ServiceCode = s.ServiceCode,
                                                ServiceName = s.ServiceName,
                                                StandardPrice = s.StandardPrice,
                                                ServiceCategoryId = sc.ServiceCategoryId,
                                                ServiceCategoryCode = sc.ServiceCategoryCode,
                                                ServiceCategoryName = sc.ServiceCategoryName
                                            }).FirstOrDefaultAsync();
        return serviceModel;
    }

    public async Task<ServiceModel?> GetServiceModelByCodeAsync(string serviceCode)
    {
        ServiceModel? serviceModel = await (from s in _Context.Services
                                            join sc in _Context.ServiceCategories on s.ServiceCategoryId equals sc.ServiceCategoryId
                                            where s.ServiceCode.Equals(serviceCode)
                                            select new ServiceModel
                                            {
                                                ServiceId = s.ServiceId,
                                                ServiceCode = s.ServiceCode,
                                                ServiceName = s.ServiceName,
                                                StandardPrice = s.StandardPrice,
                                                ServiceCategoryId = sc.ServiceCategoryId,
                                                ServiceCategoryCode = sc.ServiceCategoryCode,
                                                ServiceCategoryName = sc.ServiceCategoryName
                                            }).FirstOrDefaultAsync();
        return serviceModel;
    }

    public async Task<IList<ServiceModel>> GetServiceModelsAsync()
    {
        IList<ServiceModel> serviceModels = await (from s in _Context.Services
                                                   join sc in _Context.ServiceCategories on s.ServiceCategoryId equals sc.ServiceCategoryId
                                                   select new ServiceModel
                                                   {
                                                       ServiceId = s.ServiceId,
                                                       ServiceCode = s.ServiceCode,
                                                       ServiceName = s.ServiceName,
                                                       StandardPrice = s.StandardPrice,
                                                       ServiceCategoryId = sc.ServiceCategoryId,
                                                       ServiceCategoryCode = sc.ServiceCategoryCode,
                                                       ServiceCategoryName = sc.ServiceCategoryName
                                                   }).ToListAsync();
        return serviceModels;
    }

    public async Task<Service?> GetServiceByIdAsync(int serviceId)
    {
        return await _Context.Services
                        .Where(s => s.ServiceId.Equals(serviceId))
                        .FirstOrDefaultAsync();

    }

    public async Task<Service> InsertServiceAsync(InsertServiceRequest request, byte serviceCategoryId)
    {
        Service newService = new()
        {
            ServiceCode = request.ServiceCode,
            ServiceName = request.ServiceName,
            ServiceCategoryId = serviceCategoryId,
            StandardPrice = request.StandardPrice
        };

        _Context.Services.Add(newService);
        await _Context.SaveChangesAsync();
        return newService;
    }

    public async Task<Service> UpdateServiceAsync(UpdateServiceRequest request, byte serviceCategoryId)
    {
        Service? existingRecord = await GetServiceByIdAsync(request.ServiceId);
        if (existingRecord == null)
            throw new FileNotFoundException($"Service with id ({request.ServiceId}) not found.");

        existingRecord.ServiceCode = request.ServiceCode;
        existingRecord.ServiceName = request.ServiceName;
        existingRecord.ServiceCategoryId = serviceCategoryId;
        existingRecord.StandardPrice = request.StandardPrice;

        _Context.Services.Update(existingRecord);
        await _Context.SaveChangesAsync();

        return existingRecord;
    }

    public async Task<int> GetMedicalServicesCountAsync()
    {
        return await _Context.Services.CountAsync();
    }
}

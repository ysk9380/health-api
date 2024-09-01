using System.Security.Claims;
using FluentValidation;
using Health.Api.Authentication;
using Health.Api.Data;
using Health.Api.Models.Customer;
using Health.Api.Models.Requests.Customer;
using Health.Api.Repositories;
using Health.Api.Repositories.Customers;

namespace Health.Api.Services.Customers;

public class CustomerAddressService
{
    private const string BaseService = "/api/customers/addresses";
    private const string CustomerAddressTableName = "customeraddress";

    internal static async Task<IResult> GetCustomerAddressModelByIdAsync(long id
            , ICustomerAddressRepository customerAddressRepository)
    {
        CustomerAddressModel? customerAddress = await customerAddressRepository.GetCustomerAddressModelByIdAsync(id);
        return Results.Ok(customerAddress);
    }

    internal static async Task<IResult> GetCustomerAddressesByCustomerIdAsync(long id
            , ICustomerAddressRepository customerAddressRepository)
    {
        IList<CustomerAddressModel> addresses = await customerAddressRepository.GetCustomerAddressModelsByCustomerIdAsync(id);
        return Results.Ok(addresses);
    }

    internal static async Task<IResult> InsertCustomerAddressAsync(InsertCustomerAddressRequest request
            , IValidator<InsertCustomerAddressRequest> requestValidator
            , IMasterRepository masterRepository
            , ICustomerAddressRepository customerAddressRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        byte addressTypeId = await masterRepository.GetAddressTypeIdAsync(request.AddressTypeCode);
        if (addressTypeId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_ADDRESS_TYPE_CODE",
                Note = $"Address type code ({request.AddressTypeCode}) is unrecognized."
            });

        byte stateId = await masterRepository.GetStateIdAsync(request.StateCode);
        if (stateId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_STATE_CODE",
                Note = $"State code ({request.StateCode}) is unrecognized."
            });

        var existingRecord = await customerAddressRepository.GetCustomerAddressByAddressTypeAsync(
            request.CustomerId
            , addressTypeId);
        if (existingRecord != null)
            return Results.Conflict<CustomerAddress>(existingRecord);

        CustomerAddress newCustomerAddress = await customerAddressRepository.InsertNewCustomerAddressAsync(
            request
            , addressTypeId
            , stateId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordNewModelAuditDataAsync<CustomerAddress>(newCustomerAddress
                , CustomerAddressTableName
                , newCustomerAddress.CustomerAddressId
                , customerId
                , appUserId);

        return Results.Created($"{BaseService}/{newCustomerAddress.CustomerAddressId}", newCustomerAddress);
    }

    internal static async Task<IResult> UpdateCustomerAddressAsync(UpdateCustomerAddressRequest request
            , IValidator<UpdateCustomerAddressRequest> requestValidator
            , IMasterRepository masterRepository
            , ICustomerAddressRepository customerAddressRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        byte addressTypeId = await masterRepository.GetAddressTypeIdAsync(request.AddressTypeCode);
        if (addressTypeId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_ADDRESS_TYPE_CODE",
                Note = $"Address type code ({request.AddressTypeCode}) is unrecognized."
            });

        byte stateId = await masterRepository.GetStateIdAsync(request.StateCode);
        if (stateId == default(byte))
            return Results.BadRequest(new
            {
                Code = "INVALID_STATE_CODE",
                Note = $"State code ({request.StateCode}) is unrecognized."
            });

        var existingRecord = await customerAddressRepository.GetCustomerAddressByIdAsync(
            request.CustomerAddressId);
        if (existingRecord == null)
            return Results.BadRequest(new
            {
                Code = "CUSTOMER_ADDRESS_RECORD_NOT_FOUND",
                Note = $"Customer Address with id ({request.CustomerAddressId}) does not exist."
            });

        CustomerAddress? matchingAddressTypeAddress
            = await customerAddressRepository.GetCustomerAddressByAddressTypeAsync(existingRecord.CustomerId
                , addressTypeId);
        if (matchingAddressTypeAddress != null
            && matchingAddressTypeAddress.CustomerAddressId != request.CustomerAddressId)
            return Results.Conflict(matchingAddressTypeAddress);

        var existingRecordCopy = new CustomerAddress(existingRecord);
        CustomerAddress updatedCustomerAddress
            = await customerAddressRepository.UpdateCustomerAddressRecordAsync(request
                , addressTypeId
                , stateId);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordEditModelAuditDataAsync<CustomerAddress>(existingRecordCopy
                , updatedCustomerAddress
                , CustomerAddressTableName
                , updatedCustomerAddress.CustomerAddressId
                , customerId
                , appUserId);

        return Results.Accepted($"{BaseService}/{updatedCustomerAddress.CustomerAddressId}", existingRecord);
    }

    internal static async Task<IResult> DeactivateCustomerAddressAsync(long id
            , ICustomerAddressRepository customerAddressRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        await customerAddressRepository.DeactivateCustomerAddressAsync(id);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordDeactivatedAuditDataAsync(CustomerAddressTableName
                , id
                , customerId
                , appUserId);

        return Results.NoContent();
    }

    public static void Register(WebApplication app)
    {
        app.MapGet(string.Join("/", BaseService, "{id}"), GetCustomerAddressModelByIdAsync)
        .WithName("GetCustomerAddressById")
        .WithDescription("Fetches customer address record using customer address id.")
        .Produces<CustomerAddressModel>(200)
        .Produces(401)
        .RequireAuthorization();

        app.MapGet(string.Join("/", BaseService, "customer/{id}"), GetCustomerAddressesByCustomerIdAsync)
        .WithName("GetCustomerAddressesByCustomerId")
        .WithDescription("Fetches customer address records using customer id.")
        .Produces<CustomerAddressModel[]>(200)
        .RequireAuthorization();

        app.MapPost($"{BaseService}", InsertCustomerAddressAsync)
        .WithName("InsertCustomerAddress")
        .WithDescription("Inserts a new customer address record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<CustomerAddress>(409)
        .Produces<CustomerAddress>(201)
        .RequireAuthorization();

        app.MapPut($"{BaseService}", UpdateCustomerAddressAsync)
        .WithName("UpdateCustomerAddress")
        .WithDescription("Inserts a new customer address record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<CustomerAddress>(202)
        .RequireAuthorization();

        app.MapDelete(string.Join("/", BaseService, "{id}"), DeactivateCustomerAddressAsync)
        .WithName("DeactivateCustomerAddress")
        .WithDescription("Deactivate a customer address record.")
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<CustomerAddress>(204)
        .RequireAuthorization();
    }
}
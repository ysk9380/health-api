using System.Security.Claims;
using FluentValidation;
using Health.Api.Authentication;
using Health.Api.Data;
using Health.Api.Models.Constants;
using Health.Api.Models.Requests.Customer;
using Health.Api.Repositories;
using Health.Api.Repositories.Customers;

namespace Health.Api.Services.Customers;

public class CustomerService
{
    private const string BaseService = "/api/customers";
    private const string PatientTableName = "customer";

    internal static async Task<IResult> InsertCustomerAsync(InsertCustomerRequest request
            , IValidator<InsertCustomerRequest> requestValidator
            , ICustomerRepository customerRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var existingRecord
            = await customerRepository.GetCustomerByShortNameAsync(request.CustomerShortName);

        if (existingRecord != null)
            return Results.Conflict<Customer>(existingRecord);


        Customer newCustomer = await customerRepository.InsertCustomerAsync(request);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordNewModelAuditDataAsync<Customer>(newCustomer
                , PatientTableName
                , newCustomer.CustomerId
                , customerId
                , appUserId);

        return Results.Created($"{BaseService}/{newCustomer.CustomerId}", newCustomer);
    }

    internal static async Task<IResult> UpdateCustomerAsync(UpdateCustomerRequest request
            , IValidator<UpdateCustomerRequest> requestValidator
            , ICustomerRepository customerRepository
            , IDataAuditHistoryRepository dataAuditHistoryRepository
            , ClaimsPrincipal userPrincipal)
    {
        var validationResult = requestValidator.Validate(request);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var matchingOtherRecord
            = await customerRepository.GetOtherCustomerWithSameShortNameAsync(request.CustomerShortName
            , request.CustomerId);
        if (matchingOtherRecord != null)
            return Results.Conflict<Customer>(matchingOtherRecord);

        var existingRecord = await customerRepository.GetCustomerByIdAsync(request.CustomerId);
        if (existingRecord == null)
            return Results.BadRequest(new
            {
                Code = "CUSTOMER_RECORD_NOT_FOUND",
                Note = $"Customer with id ({request.CustomerId}) does not exist."
            });
        var existingRecordCopy = new Customer(existingRecord);

        Customer updatedRecord = await customerRepository.UpdateCustomerAsync(request);

        var (customerId, appUserId) = TokenManager.GetCustomerAndAppUser(userPrincipal);
        await dataAuditHistoryRepository.RecordEditModelAuditDataAsync(existingRecordCopy
                , updatedRecord
                , PatientTableName
                , updatedRecord.CustomerId
                , customerId
                , appUserId);
        return Results.Accepted($"{BaseService}/{updatedRecord.CustomerId}", updatedRecord);
    }

    internal static async Task<IResult> GetCustomerByIdAsync(long id
            , ICustomerRepository customerRepository)
    {
        Customer? customer = await customerRepository.GetCustomerByIdAsync(id);
        return Results.Ok(customer);
    }

    internal static async Task<IResult> GetCustomerByCodeAsync(string code
            , ICustomerRepository customerRepository)
    {
        Customer? customer = await customerRepository.GetCustomerByCodeAsync(code);
        return Results.Ok(customer);
    }

    internal static async Task<IResult> GetLatestCustomersAsync(ICustomerRepository customerRepository)
    {
        IList<Customer> customers = await customerRepository.GetLatestCustomersAsync();
        return Results.Ok(customers);
    }

    public static void Register(WebApplication app)
    {
        app.MapPost(BaseService, InsertCustomerAsync)
        .WithName("InsertCustomer")
        .WithDescription("Inserts a new customer record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<Customer>(409)
        .Produces<Customer>(201)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapPut(BaseService, UpdateCustomerAsync)
        .WithName("UpdateCustomer")
        .WithDescription("Updates an existing customer record.")
        .Produces<HttpValidationProblemDetails>(400)
        .Produces<dynamic>(400)
        .Produces(401)
        .Produces<Customer>(409)
        .Produces<Customer>(202)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapGet(string.Join("/", BaseService, "{id}"), GetCustomerByIdAsync)
        .WithName("GetCustomerById")
        .WithDescription("Get customer information using customer id.")
        .Produces<Customer>(200)
        .Produces(401)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapGet(string.Join("/", BaseService, "code/{code}"), GetCustomerByCodeAsync)
        .WithName("GetCustomerByCustomerCode")
        .WithDescription("Get customer information using customer code.")
        .Produces<Customer>(200)
        .Produces(401)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        app.MapGet(BaseService, GetLatestCustomersAsync)
        .WithName("GetLatestCustomers")
        .WithDescription("Get customer information using customer code.")
        .Produces<Customer>(200)
        .Produces(401)
        .RequireAuthorization(new string[] { AuthPolicy.SystemAdmin });

        CustomerAddressService.Register(app);
    }
}
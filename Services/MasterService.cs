using Health.Api.Data;
using Health.Api.Repositories;

namespace Health.Api.Services;

public static class MasterService
{
    private const string BaseService = "/api/master";

    internal static async Task<IResult> GetMarketsAsync(IMasterRepository masterRepository)
    {
        IList<Market> markets = await masterRepository.GetMarketsAsync();
        return Results.Ok(markets);
    }

    internal static async Task<IResult> GetGendersAsync(IMasterRepository masterRepository)
    {
        IList<Gender> genders = await masterRepository.GetGendersAsync();
        return Results.Ok(genders);
    }

    internal static async Task<IResult> GetAddressTypesAsync(IMasterRepository masterRepository)
    {
        IList<AddressType> addressTypes = await masterRepository.GetAddressTypesAsync();
        return Results.Ok(addressTypes);
    }

    internal static async Task<IResult> GetPhoneTypesAsync(IMasterRepository masterRepository)
    {
        IList<PhoneType> phoneTypes = await masterRepository.GetPhoneTypesAsync();
        return Results.Ok(phoneTypes);
    }

    internal static async Task<IResult> GetIdentityTypesAsync(IMasterRepository masterRepository)
    {
        IList<IdentityType> identityTypes = await masterRepository.GetIdentityTypesAsync();
        return Results.Ok(identityTypes);
    }

    internal static async Task<IResult> GetStatesAsync(IMasterRepository masterRepository)
    {
        IList<State> states = await masterRepository.GetStatesAsync();
        return Results.Ok(states);
    }

    internal static async Task<IResult> GetLanguagesAsync(IMasterRepository masterRepository)
    {
        IList<Language> languages = await masterRepository.GetLanguagesAsync();
        return Results.Ok(languages);
    }

    internal static async Task<IResult> GetAppUserRolesAsync(IMasterRepository masterRepository)
    {
        IList<AppUserRole> appUserRoles = await masterRepository.GetAppUserRolesAsync();
        return Results.Ok(appUserRoles);
    }

    internal static async Task<IResult> GetServiceCategoriesAsync(IMasterRepository masterRepository)
    {
        IList<ServiceCategory> serviceCategories = await masterRepository.GetServiceCategoriesAsync();
        return Results.Ok(serviceCategories);
    }

    internal static async Task<IResult> GetHealthVitalsAsync(IMasterRepository masterRepository)
    {
        IList<HealthVital> healthVitals = await masterRepository.GetHealthVitalsAsync();
        return Results.Ok(healthVitals);
    }

    public static void Register(WebApplication app)
    {
        app.MapGet($"{BaseService}/markets", GetMarketsAsync)
        .WithName("GetMarkets")
        .WithDescription("Fetches market records.")
        .Produces<Gender[]>(200);

        app.MapGet($"{BaseService}/genders", GetGendersAsync)
        .WithName("GetGenders")
        .WithDescription("Fetches gender records.")
        .Produces<Gender[]>(200);

        app.MapGet($"{BaseService}/addresstypes", GetAddressTypesAsync)
        .WithName("GetAddressTypes")
        .WithDescription("Fetches address type records.")
        .Produces<AddressType[]>(200);

        app.MapGet($"{BaseService}/phonetypes", GetPhoneTypesAsync)
        .WithName("GetPhoneTypes")
        .WithDescription("Fetches phone type records.")
        .Produces<PhoneType[]>(200);

        app.MapGet($"{BaseService}/identitytypes", GetIdentityTypesAsync)
        .WithName("GetIdentityTypes")
        .WithDescription("Fetches identity type records.")
        .Produces<IdentityType[]>(200);

        app.MapGet($"{BaseService}/states", GetStatesAsync)
        .WithName("GetStates")
        .WithDescription("Fetches state records.")
        .Produces<State[]>(200);

        app.MapGet($"{BaseService}/languages", GetLanguagesAsync)
        .WithName("GetLanguages")
        .WithDescription("Fetches language records.")
        .Produces<State[]>(200);

        app.MapGet($"{BaseService}/roles", GetAppUserRolesAsync)
        .WithName("GetUserRoles")
        .WithDescription("Fetches application user role records.")
        .Produces<AppUserRole[]>(200);

        app.MapGet($"{BaseService}/servicecategories", GetServiceCategoriesAsync)
                .WithName("GetServiceCategories")
                .WithDescription("Fetches the list of service categories.")
                .Produces<ServiceCategory[]>(200);

        app.MapGet($"{BaseService}/healthvitals", GetHealthVitalsAsync)
                .WithName("GetHealthVitals")
                .WithDescription("Fetches the list of health vitals.")
                .Produces<HealthVital[]>(200);
    }


}
using FluentValidation;
using Health.Api.Data;
using Health.Api.Models.Requests;

namespace Health.Api.Services;

public static class MarketService
{
    private const string BaseService = "/api/contacts";

    public static void Register(WebApplication app)
    {
        app.MapPost(BaseService, async (ContactRequest request
            , IValidator<ContactRequest> contactRequestValidator
            , HASDbContext context) =>
        {
            var validationResult = contactRequestValidator.Validate(request);
            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            var newContact = new Contact
            {
                MarketId = request.MarketId,
                Name = request.Name.Trim(),
                Mobile = request.Mobile.Trim(),
                Email = request.Email.Trim(),
                Address = request.Address?.Trim(),
                ArrangeDemo = request.ArrangeDemo,
                Note = request.Note?.Trim(),
                CreatedTimestamp = DateTime.Now,
                IsClosed = false
            };

            context.Contacts.Add(newContact);
            await context.SaveChangesAsync();
            return Results.Created($"{BaseService}/contacts/{newContact.ContactId}", newContact);
        })
        .WithName("InsertContact")
        .WithDescription("Inserts a new contact record.")
        .Produces<Contact>(201);
    }


}
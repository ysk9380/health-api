using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("AppUserAddress")]
public class AppUserAddress
{
    public long AppUserAddressId { get; set; }
    public long AppUserId { get; set; }
    public byte AddressTypeId { get; set; }
    public string AddressLine1 { get; set; } = default!;
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string City { get; set; } = default!;
    public string Pincode { get; set; } = default!;
    public byte StateId { get; set; }
    public bool IsActive { get; set; }

    public AppUserAddress(AppUserAddress address)
    {
        AppUserAddressId = address.AppUserAddressId;
        AppUserId = address.AppUserId;
        AddressTypeId = address.AddressTypeId;
        AddressLine1 = address.AddressLine1;
        AddressLine2 = address.AddressLine2;
        AddressLine3 = address.AddressLine3;
        City = address.City;
        Pincode = address.Pincode;
        StateId = address.StateId;
        IsActive = address.IsActive;
    }

    public AppUserAddress() { }
}
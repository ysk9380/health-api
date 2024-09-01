using System.ComponentModel.DataAnnotations.Schema;

namespace Health.Api.Data;

[Table("PatientAddress")]
public class PatientAddress
{
    public long PatientAddressId { get; set; }
    public long PatientId { get; set; }
    public byte AddressTypeId { get; set; }
    public string AddressLine1 { get; set; } = default!;
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string City { get; set; } = default!;
    public string Pincode { get; set; } = default!;
    public byte StateId { get; set; }
    public bool IsActive { get; set; }

    public PatientAddress(PatientAddress address)
    {
        PatientAddressId = address.PatientAddressId;
        PatientId = address.PatientId;
        AddressTypeId = address.AddressTypeId;
        AddressLine1 = address.AddressLine1;
        AddressLine2 = address.AddressLine2;
        AddressLine3 = address.AddressLine3;
        City = address.City;
        Pincode = address.Pincode;
        StateId = address.StateId;
        IsActive = address.IsActive;
    }

    public PatientAddress() { }
}
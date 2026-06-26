using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RentingBooking.Models;

public class PropertyFeature : BaseEntity
{
    public Guid PropertyId { get; set; }
    public int Bedrooms { get; set; } = 1;
    public int Bathrooms { get; set; } = 1;
    public int MaxGuests { get; set; } = 2;
    public int Beds { get; set; } = 1;
    public bool HasWifi { get; set; } = true;
    public bool HasAC { get; set; } = false;
    public bool HasKitchen { get; set; } = false;
    public bool HasParking { get; set; } = false;
    public bool HasTV { get; set; } = false;
    public bool HasPool { get; set; } = false;

    [ValidateNever]
    public virtual Property Property { get; set; } = null!;
}

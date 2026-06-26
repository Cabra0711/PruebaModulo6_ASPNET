using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RentingBooking.Models;

public class Property : BaseEntity
{
    public Guid HostId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Location { get; set; } = null!; 
    public decimal PricePerNight { get; set; }
    public bool IsActive { get; set; } = true;

    [ValidateNever]
    public byte[] RowVersion { get; set; } = null!;

    [ValidateNever]
    public virtual User Host { get; set; } = null!;
    public virtual ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual PropertyFeature? Features { get; set; }
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RentingBooking.Models;

public class PropertyImage : BaseEntity
{
    public Guid PropertyId { get; set; }
    public string Url { get; set; } = null!;
    public int Order { get; set; } 

    [ValidateNever]
    public virtual Property Property { get; set; } = null!;
}
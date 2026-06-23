namespace RentingBooking.Models;

public class PropertyImage : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PropertyId { get; set; }
    public string Url { get; set; } = null!;
    public int Order { get; set; } 

    public virtual Property Property { get; set; } = null!;
}
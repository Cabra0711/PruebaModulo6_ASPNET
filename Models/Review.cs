namespace RentingBooking.Models;

public class Review : BaseEntity
{
    public Guid PropertyId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = null!;

    public virtual Property Property { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

namespace RentingBooking.Models;

public class WishListItem : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid PropertyId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    public virtual User User { get; set; } = null!;
    public virtual Property Property { get; set; } = null!;
}
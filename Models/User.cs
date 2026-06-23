using RentingBooking.Enum;

namespace RentingBooking.Models;

public class User : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

 
    public virtual KycVerification? KycVerification { get; set; }
    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<WishListItem> Wishlist { get; set; } = new List<WishListItem>();
}
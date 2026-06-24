using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using RentingBooking.Enum;

namespace RentingBooking.Models;

public class User : BaseEntity
{
    public string Username { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.User;
    public string? Token { get; set; }
 
    public virtual KycVerification? KycVerification { get; set; }
    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<WishListItem> Wishlist { get; set; } = new List<WishListItem>();
}
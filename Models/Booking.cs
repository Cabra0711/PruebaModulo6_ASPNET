using RentingBooking.Enum;

namespace RentingBooking.Models;

public class Booking : BaseEntity
{
    public Guid PropertyId { get; set; }
    public Guid GuestId { get; set; }
    

    public DateOnly CheckInDate { get; set; } 
    public DateOnly CheckOutDate { get; set; } 
    
    public decimal PricePerNightAtBooking { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;
    
    public virtual Property Property { get; set; } = null!;
    public virtual User Guest { get; set; } = null!;
}
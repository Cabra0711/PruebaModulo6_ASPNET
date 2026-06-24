using RentingBooking.Models;
using RentingBooking.Response;

namespace RentingBooking.Service.Interfaces;

public interface IBookingService
{
    Task<ServiceResponse<Booking>> CreateBooking(Guid propertyId, Guid guestId, DateOnly checkIn, DateOnly checkOut);
    Task<ServiceResponse<IEnumerable<Booking>>> GetBookingsByUser(Guid userId);
    Task<ServiceResponse<IEnumerable<Booking>>> GetBookingsByProperty(Guid propertyId, Guid hostId);
    Task<ServiceResponse<bool>> CancelBooking(Guid bookingId, Guid userId);
    Task<ServiceResponse<Booking>> GetBookingById(Guid bookingId);
}

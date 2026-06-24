using RentingBooking.Models;
using RentingBooking.Response;

namespace RentingBooking.Service.Interfaces;

public interface IUserService
{
    Task<ServiceResponse<User>> GetUserByIdAsync(Guid userId);
    Task<ServiceResponse<IEnumerable<Booking>>> GetBookingsByUserAsync(Guid userId);
}

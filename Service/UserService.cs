using Microsoft.EntityFrameworkCore;
using RentingBooking.Data;
using RentingBooking.Models;
using RentingBooking.Response;
using RentingBooking.Service.Interfaces;

namespace RentingBooking.Service;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;

    public UserService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResponse<User>> GetUserByIdAsync(Guid userId)
    {
        var response = new ServiceResponse<User>();
        var user = await _dbContext.Users.Include(u => u.KycVerification).Include(u => u.Bookings).ThenInclude(b => b.Property).ThenInclude(p => p.Images).FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            response.Success = false;
            response.Message = "User not found.";
            return response;
        }

        response.Success = true;
        response.Data = user;
        return response;
    }

    public async Task<ServiceResponse<IEnumerable<Booking>>> GetBookingsByUserAsync(Guid userId)
    {
        var response = new ServiceResponse<IEnumerable<Booking>>();
        var bookings = await _dbContext.Bookings.Include(b => b.Property).ThenInclude(p => p.Images).Where(b => b.GuestId == userId).ToListAsync();

        response.Success = true;
        response.Data = bookings;
        return response;
    }
}

using Microsoft.EntityFrameworkCore;
using RentingBooking.Data;
using RentingBooking.Enum;
using RentingBooking.Models;
using RentingBooking.Response;
using RentingBooking.Service.Interfaces;

namespace RentingBooking.Service;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _dbContext;

    public BookingService(ApplicationDbContext db)
    {
        _dbContext = db;
    }

    public async Task<ServiceResponse<Booking>> CreateBooking(Guid propertyId, Guid guestId, DateOnly checkIn, DateOnly checkOut)
    {
        var response = new ServiceResponse<Booking>();

        var property = await _dbContext.Properties.FirstOrDefaultAsync(p => p.Id == propertyId && p.IsActive);
        if (property == null)
        {
            response.Success = false;
            response.Message = "Property not found or inactive.";
            return response;
        }

        var guest = await _dbContext.Users
            .Include(u => u.KycVerification)
            .FirstOrDefaultAsync(u => u.Id == guestId);

        if (guest == null)
        {
            response.Success = false;
            response.Message = "Guest not found.";
            return response;
        }

        if (guest.KycVerification == null || guest.KycVerification.Status != KycStatus.Accepted)
        {
            response.Success = false;
            response.Message = "KYC must be accepted before creating a booking.";
            return response;
        }

        var conflict = await _dbContext.Bookings.AnyAsync(b =>
            b.PropertyId == propertyId &&
            b.Status != BookingStatus.Canceled &&
            b.CheckInDate < checkOut &&
            b.CheckOutDate > checkIn);

        if (conflict)
        {
            response.Success = false;
            response.Message = "The selected dates conflict with an existing booking.";
            return response;
        }

        var booking = new Booking
        {
            PropertyId = propertyId,
            GuestId = guestId,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            PricePerNightAtBooking = property.PricePerNight,
            TotalPrice = property.PricePerNight * (checkOut.DayNumber - checkIn.DayNumber),
            Status = BookingStatus.PendingPayment
        };

        await _dbContext.Bookings.AddAsync(booking);
        await _dbContext.SaveChangesAsync();

        response.Success = true;
        response.Data = booking;
        response.Message = "Booking created successfully.";
        return response;
    }

    public async Task<ServiceResponse<IEnumerable<Booking>>> GetBookingsByUser(Guid userId)
    {
        var bookings = await _dbContext.Bookings
            .Include(b => b.Property)
            .Where(b => b.GuestId == userId)
            .ToListAsync();

        return new ServiceResponse<IEnumerable<Booking>>
        {
            Data = bookings,
            Success = true,
            Message = "User bookings retrieved successfully."
        };
    }

    public async Task<ServiceResponse<IEnumerable<Booking>>> GetBookingsByProperty(Guid propertyId, Guid hostId)
    {
        var property = await _dbContext.Properties.FirstOrDefaultAsync(p => p.Id == propertyId && p.HostId == hostId);
        if (property == null)
        {
            return new ServiceResponse<IEnumerable<Booking>>
            {
                Success = false,
                Message = "Property not found or you are not authorized to view its bookings."
            };
        }

        var bookings = await _dbContext.Bookings
            .Include(b => b.Guest)
            .Where(b => b.PropertyId == propertyId)
            .ToListAsync();

        return new ServiceResponse<IEnumerable<Booking>>
        {
            Data = bookings,
            Success = true,
            Message = "Bookings for property retrieved successfully."
        };
    }

    public async Task<ServiceResponse<bool>> CancelBooking(Guid bookingId, Guid userId)
    {
        var response = new ServiceResponse<bool>();
        var booking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            response.Success = false;
            response.Message = "Booking not found.";
            return response;
        }

        if (booking.GuestId != userId)
        {
            response.Success = false;
            response.Message = "Only the guest who created the booking can cancel it.";
            return response;
        }

        if (booking.Status != BookingStatus.PendingPayment && booking.Status != BookingStatus.Paid)
        {
            response.Success = false;
            response.Message = "Only bookings with status PendingPayment or Paid can be canceled.";
            return response;
        }

        booking.Status = BookingStatus.Canceled;
        await _dbContext.SaveChangesAsync();

        response.Success = true;
        response.Data = true;
        response.Message = "Booking canceled successfully.";
        return response;
    }

    public async Task<ServiceResponse<Booking>> GetBookingById(Guid bookingId)
    {
        var booking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
        {
            return new ServiceResponse<Booking>
            {
                Success = false,
                Message = "Booking not found."
            };
        }

        return new ServiceResponse<Booking>
        {
            Success = true,
            Data = booking,
            Message = "Booking retrieved successfully."
        };
    }
}

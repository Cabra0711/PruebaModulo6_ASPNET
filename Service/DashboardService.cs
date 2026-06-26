using System.Globalization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RentingBooking.Data;
using RentingBooking.Models;
using RentingBooking.Response;
using RentingBooking.Service.Interfaces;

namespace RentingBooking.Service;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;

    public DashboardService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResponse<DashboardViewModel>> GetDashboard(Guid hostId, Guid? propertyId, DateTime? from, DateTime? to)
    {
        var query = _db.Bookings
            .Include(b => b.Property)
            .Where(b => b.Property.HostId == hostId);

        if (propertyId.HasValue)
            query = query.Where(b => b.PropertyId == propertyId.Value);

        if (from.HasValue)
            query = query.Where(b => b.CheckInDate >= DateOnly.FromDateTime(from.Value.Date));

        if (to.HasValue)
            query = query.Where(b => b.CheckOutDate <= DateOnly.FromDateTime(to.Value.Date));

        var bookings = await query.ToListAsync();
        var properties = await _db.Properties.Where(p => p.HostId == hostId).ToListAsync();

        var totalBookings = bookings.Count;
        var totalRevenue = bookings.Sum(b => b.Property.PricePerNight * (decimal)(b.CheckOutDate.DayNumber - b.CheckInDate.DayNumber));
        var totalRooms = properties.Count;
        var occupiedDays = bookings.Sum(b => b.CheckOutDate.DayNumber - b.CheckInDate.DayNumber);

        var rangeStart = from.HasValue ? from.Value.Date : DateTime.UtcNow.AddMonths(-1).Date;
        var rangeEnd = to.HasValue ? to.Value.Date : DateTime.UtcNow.Date;
        if (rangeEnd < rangeStart)
            rangeEnd = rangeStart;

        var totalAvailableDays = totalRooms * (rangeEnd - rangeStart).TotalDays;
        var occupancyRate = totalAvailableDays > 0 ? Math.Round((occupiedDays / totalAvailableDays) * 100, 2) : 0;

        var upcomingBookingsCount = await _db.Bookings
            .Include(b => b.Property)
            .Where(b => b.Property.HostId == hostId && b.CheckInDate >= DateOnly.FromDateTime(DateTime.Today) && b.Status != Enum.BookingStatus.Canceled)
            .CountAsync();

        var revenueByMonth = bookings
            .Where(b => b.Status != Enum.BookingStatus.Canceled)
            .GroupBy(b => new { b.CheckInDate.Year, b.CheckInDate.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .ToDictionary(
                g => $"{g.Key.Year}-{g.Key.Month:D2}",
                g => g.Sum(b => b.Property.PricePerNight * (decimal)(b.CheckOutDate.DayNumber - b.CheckInDate.DayNumber))
            );

        var model = new DashboardViewModel
        {
            TotalBookings = totalBookings,
            TotalRevenue = totalRevenue,
            OccupancyRate = occupancyRate,
            Properties = properties,
            RecentBookings = bookings.OrderByDescending(b => b.CreatedAt).Take(10),
            SelectedPropertyId = propertyId,
            From = from,
            To = to,
            RevenueByMonth = revenueByMonth,
            UpcomingBookingsCount = upcomingBookingsCount
        };

        return new ServiceResponse<DashboardViewModel>
        {
            Data = model,
            Success = true,
            Message = "Dashboard data retrieved successfully."
        };
    }

    public async Task<byte[]> ExportBookingsToExcel(Guid hostId, Guid? propertyId)
    {
        var bookingsQuery = _db.Bookings
            .Include(b => b.Property)
            .Include(b => b.Guest)
            .Where(b => b.Property.HostId == hostId);

        if (propertyId.HasValue)
            bookingsQuery = bookingsQuery.Where(b => b.PropertyId == propertyId.Value);

        var bookings = await bookingsQuery.OrderByDescending(b => b.CreatedAt).ToListAsync();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Bookings");

        sheet.Cells[1, 1].Value = "Booking Id";
        sheet.Cells[1, 2].Value = "Property";
        sheet.Cells[1, 3].Value = "Guest";
        sheet.Cells[1, 4].Value = "From";
        sheet.Cells[1, 5].Value = "To";
        sheet.Cells[1, 6].Value = "Status";
        sheet.Cells[1, 7].Value = "Total Nights";
        sheet.Cells[1, 8].Value = "Revenue";

        var row = 2;
        if (bookings.Count == 0)
        {
            sheet.Cells[row, 1].Value = "Sin reservas";
            sheet.Cells[row, 2].Value = "";
            sheet.Cells[row, 3].Value = "";
            sheet.Cells[row, 4].Value = "";
            sheet.Cells[row, 5].Value = "";
            sheet.Cells[row, 6].Value = "";
            sheet.Cells[row, 7].Value = "";
            sheet.Cells[row, 8].Value = "";
        }
        else
        {
            foreach (var booking in bookings)
            {
                var nights = booking.CheckOutDate.DayNumber - booking.CheckInDate.DayNumber;
                sheet.Cells[row, 1].Value = booking.Id.ToString();
                sheet.Cells[row, 2].Value = booking.Property?.Title;
                sheet.Cells[row, 3].Value = booking.Guest?.Username ?? booking.Guest?.Email;
                sheet.Cells[row, 4].Value = booking.CheckInDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                sheet.Cells[row, 5].Value = booking.CheckOutDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                sheet.Cells[row, 6].Value = booking.Status.ToString();
                sheet.Cells[row, 7].Value = nights;
                sheet.Cells[row, 8].Value = booking.Property?.PricePerNight * nights;
                row++;
            }
        }

        if (sheet.Dimension != null)
        {
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        return await package.GetAsByteArrayAsync();
    }
}

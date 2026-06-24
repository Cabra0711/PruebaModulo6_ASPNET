using RentingBooking.Models;
using RentingBooking.Response;

namespace RentingBooking.Service.Interfaces;

public interface IDashboardService
{
    Task<ServiceResponse<DashboardViewModel>> GetDashboard(Guid hostId, Guid? propertyId, DateTime? from, DateTime? to);
    Task<byte[]> ExportBookingsToExcel(Guid hostId, Guid? propertyId);
}

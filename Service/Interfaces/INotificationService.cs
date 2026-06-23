using RentingBooking.Response;
using RentingBooking.Service;

namespace RentingBooking.Service.Interfaces;

public interface INotificationService
{
    Task<ServiceResponse<bool>> SendNotificationAsync(NotificationRequest request);
}

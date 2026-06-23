using RentingBooking.Models;
using RentingBooking.Response;

namespace RentingBooking.Service.Interfaces;

public interface IAuthService
{
    public Task<ServiceResponse<User>> Login(string username, string password);
    public Task<ServiceResponse<User>> RegisterCustomer(User user);
    public Task<ServiceResponse<User>> RegisterOwner(User user);
}
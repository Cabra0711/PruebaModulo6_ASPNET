using RentingBooking.Models;
using RentingBooking.Response;

namespace RentingBooking.Service.Interfaces;

public interface IWishlistService
{
    Task<ServiceResponse<IEnumerable<WishListItem>>> GetWishlistByUser(Guid userId);
    Task<ServiceResponse<WishListItem>> AddToWishlist(Guid userId, Guid propertyId);
    Task<ServiceResponse<bool>> RemoveFromWishlist(Guid userId, Guid propertyId);
    Task<ServiceResponse<bool>> IsInWishlist(Guid userId, Guid propertyId);
}

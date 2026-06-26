using RentingBooking.Models;
using RentingBooking.Response;

namespace RentingBooking.Service.Interfaces;

public interface IReviewService
{
    Task<ServiceResponse<Review>> CreateReview(Guid propertyId, Guid userId, int rating, string comment);
    Task<ServiceResponse<IEnumerable<Review>>> GetReviewsByProperty(Guid propertyId, int take = 3);
    Task<ServiceResponse<double>> GetAverageRating(Guid propertyId);
    Task<ServiceResponse<int>> GetReviewCount(Guid propertyId);
    Task<ServiceResponse<bool>> HasUserReviewed(Guid propertyId, Guid userId);
}

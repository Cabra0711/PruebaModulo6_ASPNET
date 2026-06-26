using Microsoft.EntityFrameworkCore;
using RentingBooking.Data;
using RentingBooking.Models;
using RentingBooking.Response;
using RentingBooking.Service.Interfaces;

namespace RentingBooking.Service;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _db;

    public ReviewService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResponse<Review>> CreateReview(Guid propertyId, Guid userId, int rating, string comment)
    {
        var response = new ServiceResponse<Review>();

        if (rating < 1 || rating > 5)
        {
            response.Success = false;
            response.Message = "Rating must be between 1 and 5.";
            return response;
        }

        var property = await _db.Properties.AnyAsync(p => p.Id == propertyId);
        if (!property)
        {
            response.Success = false;
            response.Message = "Property not found.";
            return response;
        }

        var existing = await _db.Reviews.AnyAsync(r => r.PropertyId == propertyId && r.UserId == userId);
        if (existing)
        {
            response.Success = false;
            response.Message = "You have already reviewed this property.";
            return response;
        }

        var review = new Review
        {
            PropertyId = propertyId,
            UserId = userId,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };

        await _db.Reviews.AddAsync(review);
        await _db.SaveChangesAsync();

        response.Success = true;
        response.Data = review;
        response.Message = "Review created successfully.";
        return response;
    }

    public async Task<ServiceResponse<IEnumerable<Review>>> GetReviewsByProperty(Guid propertyId, int take = 3)
    {
        var reviews = await _db.Reviews
            .Include(r => r.User)
            .Where(r => r.PropertyId == propertyId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(take)
            .ToListAsync();

        return new ServiceResponse<IEnumerable<Review>>
        {
            Data = reviews,
            Success = true,
            Message = "Reviews retrieved successfully."
        };
    }

    public async Task<ServiceResponse<double>> GetAverageRating(Guid propertyId)
    {
        var avg = await _db.Reviews
            .Where(r => r.PropertyId == propertyId)
            .AverageAsync(r => (double?)r.Rating) ?? 0;

        return new ServiceResponse<double>
        {
            Data = Math.Round(avg, 1),
            Success = true
        };
    }

    public async Task<ServiceResponse<int>> GetReviewCount(Guid propertyId)
    {
        var count = await _db.Reviews
            .Where(r => r.PropertyId == propertyId)
            .CountAsync();

        return new ServiceResponse<int>
        {
            Data = count,
            Success = true
        };
    }

    public async Task<ServiceResponse<bool>> HasUserReviewed(Guid propertyId, Guid userId)
    {
        var exists = await _db.Reviews.AnyAsync(r => r.PropertyId == propertyId && r.UserId == userId);
        return new ServiceResponse<bool> { Data = exists, Success = true };
    }
}

using Microsoft.EntityFrameworkCore;
using RentingBooking.Data;
using RentingBooking.Models;
using RentingBooking.Response;
using RentingBooking.Service.Interfaces;

namespace RentingBooking.Service;

public class WishlistService : IWishlistService
{
    private readonly ApplicationDbContext _dbContext;

    public WishlistService(ApplicationDbContext db)
    {
        _dbContext = db;
    }

    public async Task<ServiceResponse<IEnumerable<WishListItem>>> GetWishlistByUser(Guid userId)
    {
        var items = await _dbContext.WishlistItems
            .Include(w => w.Property)
            .Where(w => w.UserId == userId)
            .ToListAsync();

        return new ServiceResponse<IEnumerable<WishListItem>>
        {
            Data = items,
            Success = true,
            Message = "Wishlist retrieved successfully."
        };
    }

    public async Task<ServiceResponse<WishListItem>> AddToWishlist(Guid userId, Guid propertyId)
    {
        var response = new ServiceResponse<WishListItem>();

        var property = await _dbContext.Properties.FirstOrDefaultAsync(p => p.Id == propertyId && p.IsActive);
        if (property == null)
        {
            response.Success = false;
            response.Message = "Property not found or inactive.";
            return response;
        }

        var exists = await _dbContext.WishlistItems.AnyAsync(w => w.UserId == userId && w.PropertyId == propertyId);
        if (exists)
        {
            response.Success = false;
            response.Message = "Property is already in your wishlist.";
            return response;
        }

        var item = new WishListItem
        {
            UserId = userId,
            PropertyId = propertyId,
            AddedAt = DateTime.UtcNow
        };

        await _dbContext.WishlistItems.AddAsync(item);
        await _dbContext.SaveChangesAsync();

        response.Success = true;
        response.Data = item;
        response.Message = "Property added to wishlist.";
        return response;
    }

    public async Task<ServiceResponse<bool>> RemoveFromWishlist(Guid userId, Guid propertyId)
    {
        var response = new ServiceResponse<bool>();
        var item = await _dbContext.WishlistItems.FirstOrDefaultAsync(w => w.UserId == userId && w.PropertyId == propertyId);

        if (item == null)
        {
            response.Success = false;
            response.Message = "Item not found in wishlist.";
            response.Data = false;
            return response;
        }

        _dbContext.WishlistItems.Remove(item);
        await _dbContext.SaveChangesAsync();

        response.Success = true;
        response.Data = true;
        response.Message = "Item removed from wishlist.";
        return response;
    }

    public async Task<ServiceResponse<bool>> IsInWishlist(Guid userId, Guid propertyId)
    {
        var exists = await _dbContext.WishlistItems.AnyAsync(w => w.UserId == userId && w.PropertyId == propertyId);

        return new ServiceResponse<bool>
        {
            Data = exists,
            Success = true,
            Message = exists ? "Property is in wishlist." : "Property is not in wishlist."
        };
    }
}

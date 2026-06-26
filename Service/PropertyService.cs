using Microsoft.EntityFrameworkCore;
using RentingBooking.Data;
using RentingBooking.Models;
using RentingBooking.Response;
using RentingBooking.Service.Interfaces;

namespace RentingBooking.Service;

public class PropertyService : IPropertyService
{
    private readonly ApplicationDbContext _dbContext;
    public PropertyService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<ServiceResponse<IEnumerable<Property>>> GetPublicProperties()
    {
        var properties = await _dbContext.Properties
            .Include(p => p.Images)
            .Include(p => p.Features)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive)
            .ToListAsync();

        return new ServiceResponse<IEnumerable<Property>>() 
        {
            Data = properties,
            Success = true,
            Message = "Public catalog retrieved successfully."
        };
    }
    
    public async Task<ServiceResponse<IEnumerable<Property>>> GetPropertiesByHost(Guid hostId)
    {
        var properties = await _dbContext.Properties
            .Include(p => p.Images)
            .Include(p => p.Features)
            .Include(p => p.Bookings)
            .Include(p => p.Reviews)
            .Where(p => p.HostId == hostId)
            .ToListAsync();

        return new ServiceResponse<IEnumerable<Property>>()
        {
            Data = properties,
            Success = true,
            Message = "Host properties retrieved successfully."
        };
    }

    public async Task<ServiceResponse<Property>> GetPropertyById(Guid id)
    {
        var response = new ServiceResponse<Property>();
        var property = await _dbContext.Properties
            .Include(p => p.Images)
            .Include(p => p.Features)
            .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
            .Include(p => p.Host)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property != null)
        {
            response.Data = property;
            response.Success = true;
            response.Message = "Property Found";
            return response;
        }
        else
        {
            response.Success = false;
            response.Message = "Property Not Found";
            return response;
        }
    }

    public async Task<ServiceResponse<Property>> CreateProperty(Property property, Guid hostId)
    {
        var response = new ServiceResponse<Property>();
        
        var exists = await _dbContext.Properties.AnyAsync(p => p.HostId == hostId && p.Title == property.Title);

        if (exists)
        {
            response.Success = false;
            response.Message = "The property already exists";
            return response;
        }

        property.HostId = hostId;
        property.IsActive = true;
        property.CreatedAt = DateTime.UtcNow;

        if (property.Images != null)
        {
            property.Images = property.Images.Where(i => !string.IsNullOrWhiteSpace(i.Url)).ToList();
        }

        if (property.Features != null)
        {
            property.Features.PropertyId = property.Id;
            property.Features.CreatedAt = DateTime.UtcNow;
        }

        await _dbContext.Properties.AddAsync(property);
        await _dbContext.SaveChangesAsync();
        
        response.Data = property;
        response.Success = true;
        response.Message = "Property created successfully.";
        return response;
    }

    public async Task<ServiceResponse<Property>> UpdateProperty(Property property, Guid id, Guid hostId)
    {
        var response = new ServiceResponse<Property>();
        var exists = await _dbContext.Properties
            .Include(p => p.Features)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (exists != null)
        {
            if (exists.HostId != hostId)
            {
                response.Success = false;
                response.Message = "Unauthorized to update this property.";
                return response;
            }

            exists.Title = property.Title;
            exists.Description = property.Description;
            exists.Location = property.Location;
            exists.PricePerNight = property.PricePerNight;
            exists.RowVersion = property.RowVersion;

            var existingImages = await _dbContext.PropertyImages.Where(i => i.PropertyId == id).ToListAsync();
            _dbContext.PropertyImages.RemoveRange(existingImages);

            if (property.Images != null)
            {
                var newImages = property.Images
                    .Where(i => !string.IsNullOrWhiteSpace(i.Url))
                    .Select(i => new PropertyImage { PropertyId = id, Url = i.Url, Order = i.Order })
                    .ToList();
                await _dbContext.PropertyImages.AddRangeAsync(newImages);
            }

            if (property.Features != null)
            {
                if (exists.Features != null)
                {
                    exists.Features.Bedrooms = property.Features.Bedrooms;
                    exists.Features.Bathrooms = property.Features.Bathrooms;
                    exists.Features.MaxGuests = property.Features.MaxGuests;
                    exists.Features.Beds = property.Features.Beds;
                    exists.Features.HasWifi = property.Features.HasWifi;
                    exists.Features.HasAC = property.Features.HasAC;
                    exists.Features.HasKitchen = property.Features.HasKitchen;
                    exists.Features.HasParking = property.Features.HasParking;
                    exists.Features.HasTV = property.Features.HasTV;
                    exists.Features.HasPool = property.Features.HasPool;
                }
                else
                {
                    property.Features.PropertyId = id;
                    property.Features.CreatedAt = DateTime.UtcNow;
                    _dbContext.PropertyFeatures.Add(property.Features);
                }
            }

            await _dbContext.SaveChangesAsync();
            
            response.Data = exists;
            response.Success = true;
            response.Message = "Property Updated successfully.";
            return response;
        }
        else
        {
            response.Success = false;
            response.Message = "Property Not Found";
            return response;
        }
    }

    public async Task<ServiceResponse<Property>> DeleteProperty(Guid id, Guid hostId)
    {
        var response = new ServiceResponse<Property>();
        var exists = await _dbContext.Properties.FindAsync(id);

        if (exists != null)
        {
            if (exists.HostId != hostId)
            {
                response.Success = false;
                response.Message = "Unauthorized to delete this property.";
                return response;
            }

            exists.IsActive = false;
            await _dbContext.SaveChangesAsync();
            
            response.Data = exists;
            response.Success = true;
            response.Message = "Property Deleted successfully.";
            return response;
        }
        else
        {
            response.Success = false;
            response.Message = "Property Not Found";
            return response;
        }
    }

    public async Task<ServiceResponse<Property>> GetFeaturedProperty()
    {
        var property = await _dbContext.Properties
            .Include(p => p.Images)
            .Include(p => p.Features)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Bookings!.Count)
            .ThenByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        return new ServiceResponse<Property>
        {
            Data = property,
            Success = property != null,
            Message = property != null ? "Featured property found." : "No properties available."
        };
    }
}

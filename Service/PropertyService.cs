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
        var property = await _dbContext.Properties.FirstOrDefaultAsync(p => p.Id == id);

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
        
        var exists = await _dbContext.Properties.AnyAsync(p => p.HostId == hostId && p.Title == property.Title );

        if (exists)
        {
            response.Success = false;
            response.Message = "The property already exists";
            return response;
        }

        property.HostId = hostId;
        property.IsActive = true;
        property.CreatedAt = DateTime.UtcNow;

        await _dbContext.Properties.AddAsync(property);
        await _dbContext.SaveChangesAsync();
        
        response.Data = property;
        response.Success = true;
        response.Message = "Property created successfully.";
        return response;
    }

    // AÑADIDO: Guid hostId en los parámetros
    public async Task<ServiceResponse<Property>> UpdateProperty(Property property, Guid id, Guid hostId)
    {
        var response = new ServiceResponse<Property>();
        var exists = await _dbContext.Properties.FindAsync(id);

        if (exists != null)
        {
            // AÑADIDO: Filtro de seguridad multitenant
            if (exists.HostId != hostId)
            {
                response.Success = false;
                response.Message = "Unauthorized to update this property.";
                return response;
            }

            exists.Title =  property.Title;
            exists.Description = property.Description;
            exists.Location = property.Location;
            exists.PricePerNight = property.PricePerNight;
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

    // AÑADIDO: Guid hostId en los parámetros
    public async Task<ServiceResponse<Property>> DeleteProperty(Guid id, Guid hostId)
    {
        var response = new ServiceResponse<Property>();
        var exists = await _dbContext.Properties.FindAsync(id);

        if (exists != null)
        {
            // AÑADIDO: Filtro de seguridad multitenant
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
}
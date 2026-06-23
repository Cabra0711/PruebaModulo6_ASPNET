using RentingBooking.Models;
using RentingBooking.Response;

namespace RentingBooking.Service.Interfaces;

public interface IPropertyService
{
    Task<ServiceResponse<IEnumerable<Property>>> GetPublicProperties();
    Task<ServiceResponse<IEnumerable<Property>>> GetPropertiesByHost(Guid hostId);
    Task<ServiceResponse<Property>> GetPropertyById(Guid id);


    Task<ServiceResponse<Property>> CreateProperty(Property property, Guid hostId);
    

    Task<ServiceResponse<Property>> UpdateProperty(Property property, Guid id, Guid hostId);
    Task<ServiceResponse<Property>> DeleteProperty(Guid id, Guid hostId);
}
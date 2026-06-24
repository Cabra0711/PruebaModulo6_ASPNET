using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentingBooking.Models;
using RentingBooking.Service.Interfaces;

namespace RentingBooking.Controllers;

[Route("BookingRenting/Property")]
public class PropertyController : Controller
{
    private readonly IPropertyService _propertyService;

    public PropertyController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [AllowAnonymous]
    [HttpGet("public")]
    public async Task<IActionResult> GetPublicProperties()
    {
        var response = await _propertyService.GetPublicProperties();
        return View(response.Data);
    }


    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPropertyById(Guid id)
    {
        var response = await _propertyService.GetPropertyById(id);
        if (!response.Success)
        {
            TempData["ErrorMessage"] = response.Message;
            return RedirectToAction(nameof(GetPublicProperties));
        }
        
        return View(response.Data);
    }


    [Authorize(Roles = "Owner")]
    [HttpGet("dashboard")]
    public IActionResult GetPropertiesByHost()
    {
        return RedirectToAction("OwnerLanding", "Owner");
    }


    [Authorize(Roles = "Owner")]
    [HttpGet("create")]
    public IActionResult CreateProperty()
    {
        return View();
    }


    [Authorize(Roles = "Owner")]
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProperty(Property property)
    {
        var hostId = GetUserIdFromToken();
        if (hostId == Guid.Empty) return RedirectToAction("Login", "Auth");


        if (!ModelState.IsValid)
        {
            return View(property);
        }

        var response = await _propertyService.CreateProperty(property, hostId);
        if (!response.Success)
        {
            TempData["ErrorMessage"] = response.Message;
            return View(property);
        }

        TempData["SuccessMessage"] = response.Message;
        return RedirectToAction(nameof(GetPropertiesByHost));
    }


    [Authorize(Roles = "Owner")]
    [HttpGet("edit/{id:guid}")]
    public async Task<IActionResult> UpdateProperty(Guid id)
    {
        var hostId = GetUserIdFromToken();
        if (hostId == Guid.Empty) return RedirectToAction("Login", "Auth");

        var response = await _propertyService.GetPropertyById(id);

        if (!response.Success || response.Data?.HostId != hostId)
        {
            TempData["ErrorMessage"] = "You are not allowed to edit this property or you are not logged in";
            return RedirectToAction(nameof(GetPropertiesByHost));
        }

        return View(response.Data);
    }

   
    [Authorize(Roles = "Owner")]
    [HttpPost("edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProperty(Guid id, Property property)
    {
        var hostId = GetUserIdFromToken();
        if (hostId == Guid.Empty) return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
        {
            return View(property);
        }

        var response = await _propertyService.UpdateProperty(property, id,  hostId);
        if (!response.Success)
        {
            TempData["ErrorMessage"] = response.Message;
            return View(property);
        }

        TempData["SuccessMessage"] = response.Message;
        return RedirectToAction(nameof(GetPropertiesByHost));
    }


    [Authorize(Roles = "Owner")]
    [HttpPost("delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProperty(Guid id)
    {
        var hostId = GetUserIdFromToken();
        if (hostId == Guid.Empty) return RedirectToAction("Login", "Auth");

        var response = await _propertyService.DeleteProperty(id, hostId);
        if (!response.Success)
        {
            TempData["ErrorMessage"] = response.Message;
        }
        else
        {
            TempData["SuccessMessage"] = response.Message;
        }

        return RedirectToAction(nameof(GetPropertiesByHost));
    }

   
    private Guid GetUserIdFromToken()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdStr, out Guid userId))
        {
            return userId;
        }
        return Guid.Empty;
    }
}
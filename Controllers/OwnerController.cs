using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentingBooking.Service.Interfaces;

namespace RentingBooking.Controllers;

[Authorize(Roles = "Owner")]
[Route("Owner")]
public class OwnerController : Controller
{
    private readonly IPropertyService _propertyService;

    public OwnerController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpGet("Landing")]
    public async Task<IActionResult> OwnerLanding()
    {
        var ownerId = GetUserIdFromToken();
        if (ownerId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var response = await _propertyService.GetPropertiesByHost(ownerId);
        if (!response.Success)
        {
            TempData["ErrorMessage"] = response.Message;
            return RedirectToAction("Login", "Auth");
        }

        return View(response.Data);
    }

    private Guid GetUserIdFromToken()
    {
        var ownerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(ownerIdStr, out var ownerId) ? ownerId : Guid.Empty;
    }
}
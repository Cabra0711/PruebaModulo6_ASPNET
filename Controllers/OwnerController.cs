using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentingBooking.Service.Interfaces;

namespace RentingBooking.Controllers;

[Authorize(Roles = "Owner")]
[Route("BookingRenting/Owner")]
public class OwnerController : Controller
{
    private readonly IPropertyService _propertyService;
    private readonly IDashboardService _dashboardService;
    private readonly IUserService _userService;

    public OwnerController(IPropertyService propertyService, IDashboardService dashboardService, IUserService userService)
    {
        _propertyService = propertyService;
        _dashboardService = dashboardService;
        _userService = userService;
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

    [HttpGet("Analytics")]
    public async Task<IActionResult> OwnerDashboard()
    {
        var ownerId = GetUserIdFromToken();
        if (ownerId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var user = await _userService.GetUserByIdAsync(ownerId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Login", "Auth");
        }

        return View(user.Data);
    }

    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard(Guid? propertyId, DateTime? from, DateTime? to)
    {
        var ownerId = GetUserIdFromToken();
        if (ownerId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var response = await _dashboardService.GetDashboard(ownerId, propertyId, from, to);
        if (!response.Success)
        {
            TempData["ErrorMessage"] = response.Message;
            return RedirectToAction("OwnerLanding");
        }

        return View(response.Data);
    }

    [HttpGet("ExportBookings")]
    public async Task<IActionResult> ExportBookings(Guid? propertyId)
    {
        var ownerId = GetUserIdFromToken();
        if (ownerId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var fileData = await _dashboardService.ExportBookingsToExcel(ownerId, propertyId);
        var fileName = $"bookings-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private Guid GetUserIdFromToken()
    {
        var ownerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(ownerIdStr, out var ownerId) ? ownerId : Guid.Empty;
    }
}
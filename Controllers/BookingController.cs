using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentingBooking.Service.Interfaces;

namespace RentingBooking.Controllers;

[Route("BookingRenting/Booking")]
public class BookingController : Controller
{
    private readonly IBookingService _bookingService;
    private readonly IPropertyService _propertyService;

    public BookingController(IBookingService bookingService, IPropertyService propertyService)
    {
        _bookingService = bookingService;
        _propertyService = propertyService;
    }

    [Authorize(Roles = "User")]
    [HttpGet("Create/{propertyId:guid}")]
    public async Task<IActionResult> CreateBooking(Guid propertyId, DateOnly? checkIn = null, DateOnly? checkOut = null)
    {
        var response = await _propertyService.GetPropertyById(propertyId);
        if (!response.Success || response.Data == null)
        {
            TempData["ErrorMessage"] = response.Message;
            return RedirectToAction("GetPublicProperties", "Property");
        }

        var property = response.Data;
        var defaultCheckIn = checkIn ?? DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        var defaultCheckOut = checkOut ?? DateOnly.FromDateTime(DateTime.Today.AddDays(12));

        ViewData["CheckIn"] = defaultCheckIn;
        ViewData["CheckOut"] = defaultCheckOut;
        return View(property);
    }

    [Authorize(Roles = "User")]
    [HttpPost("Create/{propertyId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBooking(Guid propertyId, DateOnly checkIn, DateOnly checkOut)
    {
        var userId = GetUserIdFromToken();
        if (userId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var response = await _bookingService.CreateBooking(propertyId, userId, checkIn, checkOut);
        if (!response.Success)
        {
            TempData["ErrorMessage"] = response.Message;
            return RedirectToAction("CreateBooking", new { propertyId });
        }

        TempData["SuccessMessage"] = response.Message;
        return RedirectToAction("MyBookings", "User");
    }

    [Authorize(Roles = "Owner")]
    [HttpGet("Property/{propertyId:guid}")]
    public async Task<IActionResult> PropertyBookings(Guid propertyId)
    {
        var ownerId = GetUserIdFromToken();
        if (ownerId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var response = await _bookingService.GetBookingsByProperty(propertyId, ownerId);
        if (!response.Success)
        {
            TempData["ErrorMessage"] = response.Message;
            return RedirectToAction("OwnerLanding", "Owner");
        }

        return View(response.Data);
    }

    [Authorize(Roles = "User")]
    [HttpPost("Cancel/{bookingId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelBooking(Guid bookingId)
    {
        var userId = GetUserIdFromToken();
        if (userId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var response = await _bookingService.CancelBooking(bookingId, userId);
        if (!response.Success)
        {
            TempData["ErrorMessage"] = response.Message;
        }
        else
        {
            TempData["SuccessMessage"] = response.Message;
        }

        return RedirectToAction("MyBookings", "User");
    }

    private Guid GetUserIdFromToken()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
    }
}

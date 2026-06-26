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
    private readonly IReviewService _reviewService;

    public BookingController(IBookingService bookingService, IPropertyService propertyService, IReviewService reviewService)
    {
        _bookingService = bookingService;
        _propertyService = propertyService;
        _reviewService = reviewService;
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
        var today = DateOnly.FromDateTime(DateTime.Today);
        var defaultCheckIn = checkIn ?? today.AddDays(7);
        var defaultCheckOut = checkOut ?? today.AddDays(12);

        if (defaultCheckIn < today)
        {
            TempData["ErrorMessage"] = "La fecha de llegada no puede ser anterior a la fecha actual.";
            return RedirectToAction("GetPropertyById", "Property", new { id = propertyId });
        }

        if (defaultCheckOut <= defaultCheckIn)
        {
            TempData["ErrorMessage"] = "La fecha de salida debe ser posterior a la fecha de llegada.";
            return RedirectToAction("GetPropertyById", "Property", new { id = propertyId });
        }

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

        var bookingsResponse = await _bookingService.GetBookingsByProperty(propertyId, ownerId);
        if (!bookingsResponse.Success)
        {
            TempData["ErrorMessage"] = bookingsResponse.Message;
            return RedirectToAction("OwnerLanding", "Owner");
        }

        var propertyResponse = await _propertyService.GetPropertyById(propertyId);
        if (!propertyResponse.Success)
        {
            TempData["ErrorMessage"] = propertyResponse.Message;
            return RedirectToAction("OwnerLanding", "Owner");
        }

        var model = (propertyResponse.Data, bookingsResponse.Data.AsEnumerable());
        return View(model);
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
    
    [Authorize(Roles = "User")]
    [HttpPost("Pay/{bookingId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PayBooking(Guid bookingId)
    {
        var userId = GetUserIdFromToken();
        if (userId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var response = await _bookingService.PayBooking(bookingId, userId);
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

    [Authorize(Roles = "Owner")]
    [HttpPost("Approve/{bookingId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveBooking(Guid bookingId, Guid propertyId)
    {
        var ownerId = GetUserIdFromToken();
        if (ownerId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var response = await _bookingService.ApproveBooking(bookingId, ownerId);
        TempData[response.Success ? "SuccessMessage" : "ErrorMessage"] =
            response.Success ? "Reserva aprobada correctamente." : response.Message;

        return RedirectToAction("PropertyBookings", new { propertyId });
    }

    [Authorize(Roles = "User")]
    [HttpPost("Review/{bookingId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReview(Guid bookingId, int rating, string comment)
    {
        var userId = GetUserIdFromToken();
        if (userId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var bookingResponse = await _bookingService.GetBookingById(bookingId);
        if (!bookingResponse.Success || bookingResponse.Data == null)
        {
            TempData["ErrorMessage"] = "Booking not found.";
            return RedirectToAction("MyBookings", "User");
        }

        var booking = bookingResponse.Data;
        if (booking.GuestId != userId)
        {
            TempData["ErrorMessage"] = "You can only review your own bookings.";
            return RedirectToAction("MyBookings", "User");
        }

        if (booking.Status != Enum.BookingStatus.Completed)
        {
            TempData["ErrorMessage"] = "You can only review completed stays.";
            return RedirectToAction("MyBookings", "User");
        }

        var response = await _reviewService.CreateReview(booking.PropertyId, userId, rating, comment);
        if (!response.Success)
        {
            TempData["ErrorMessage"] = response.Message;
        }
        else
        {
            TempData["SuccessMessage"] = "Review submitted successfully!";
        }

        return RedirectToAction("MyBookings", "User");
    }
}

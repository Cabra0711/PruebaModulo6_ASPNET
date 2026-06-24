using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentingBooking.Service.Interfaces;

namespace RentingBooking.Controllers;

[Authorize(Roles = "User")]
[Route("BookingRenting/User")]
public class UserController : Controller
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("Landing")]
    public async Task<IActionResult> UserLanding()
    {
        var userId = GetUserIdFromToken();
        if (userId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var response = await _userService.GetUserByIdAsync(userId);
        if (!response.Success)
        {
            TempData["ErrorMessage"] = response.Message;
            return RedirectToAction("Login", "Auth");
        }

        return View(response.Data);
    }

    [HttpGet("Bookings")]
    public async Task<IActionResult> MyBookings()
    {
        var userId = GetUserIdFromToken();
        if (userId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var response = await _userService.GetBookingsByUserAsync(userId);
        return View(response.Data);
    }

    private Guid GetUserIdFromToken()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
    }
}
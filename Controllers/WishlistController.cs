using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentingBooking.Service.Interfaces;

namespace RentingBooking.Controllers;

[Authorize(Roles = "User")]
[Route("Wishlist")]
public class WishlistController : Controller
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserIdFromToken();
        if (userId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var response = await _wishlistService.GetWishlistByUser(userId);
        if (!response.Success)
        {
            TempData["ErrorMessage"] = response.Message;
            return View(Enumerable.Empty<object>());
        }

        return View(response.Data);
    }

    [HttpPost("Add/{propertyId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Guid propertyId)
    {
        var userId = GetUserIdFromToken();
        if (userId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var response = await _wishlistService.AddToWishlist(userId, propertyId);
        if (!response.Success)
            TempData["ErrorMessage"] = response.Message;
        else
            TempData["SuccessMessage"] = response.Message;

        return RedirectToAction("Index");
    }

    [HttpPost("Remove/{propertyId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid propertyId)
    {
        var userId = GetUserIdFromToken();
        if (userId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var response = await _wishlistService.RemoveFromWishlist(userId, propertyId);
        if (!response.Success)
            TempData["ErrorMessage"] = response.Message;
        else
            TempData["SuccessMessage"] = response.Message;

        return RedirectToAction("Index");
    }

    private Guid GetUserIdFromToken()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
    }
}

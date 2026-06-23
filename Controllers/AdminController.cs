using Microsoft.AspNetCore.Mvc;

namespace RentingBooking.Controllers;

public class AdminController : Controller
{
    public async Task<IActionResult> Admin()
    {
        return View();
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RentingBooking.Controllers;

[Authorize]
public class UserController : Controller
{
    [HttpGet("User")]
    public IActionResult UserLanding()
    {
        return View();
    }
}
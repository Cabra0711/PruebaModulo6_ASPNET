using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RentingBooking.Controllers;

[Authorize]
public class OwnerController : Controller
{
    [HttpGet("OwnerLanding")]
    public async Task<IActionResult> OwnerLanding()
    {
        return View();
    }
}
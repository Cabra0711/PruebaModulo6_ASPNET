using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RentingBooking.Models;
using RentingBooking.Service.Interfaces;

namespace RentingBooking.Controllers;

public class HomeController : Controller
{
    private readonly IPropertyService _propertyService;

    public HomeController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    public async Task<IActionResult> Index()
    {
        var featured = await _propertyService.GetFeaturedProperty();
        return View(featured.Data);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

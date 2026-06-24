using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using RentingBooking.Enum;
using RentingBooking.Models;
using RentingBooking.Service.Interfaces;

namespace RentingBooking.Controllers;

[Route("BookingRenting")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [AllowAnonymous]
    [HttpGet("Login")]
    public async Task<IActionResult> Login()
    {
        return View();
    }
    
    [HttpPost("Login")]
    public async Task<IActionResult> Login(string username, string password)
    {
        var response = await _authService.Login(username, password);
        
        if (response.Success)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, response.Data.Id.ToString()),
                new Claim(ClaimTypes.Name, response.Data.Username),
                new Claim(ClaimTypes.Role, response.Data.Role.ToString())
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            HttpContext.Session.SetString("Username", username);
            if (!string.IsNullOrEmpty(response.Data.Token))
            {
                HttpContext.Session.SetString("Token", response.Data.Token);
            }

            if (response.Data.Role == UserRole.User)
            {
                return RedirectToAction("UserLanding", "User");
            }
            else if (response.Data.Role == UserRole.Owner)
            {
                return RedirectToAction("OwnerLanding", "Owner");
            }
            else
            {
                return RedirectToAction("Admin", "Admin");
            }
        }
        ViewBag.Error = response?.Message ?? "Credenciales incorrectas o invalidas intente de nuevo porfavor.";
        return View();
    }
    
    [AllowAnonymous]
    [HttpPost("RegisterCustomer")]
    public async Task<IActionResult> RegisterCustomer(User user)     
    {         
        var validator = new Validators.UserValidator();         
        var validationResult = await validator.ValidateAsync(user);          
        
        if (!validationResult.IsValid)         
        {             
            ViewBag.ErrorValidation = string.Join("<br/>", validationResult.Errors.Select(e => e.ErrorMessage));             
            return View("Login", user);         
        }          
        
        try         
        {             
            var response = await _authService.RegisterCustomer(user);             
            if (response.Success)             
            {                 
                ViewBag.Success = "¡Account Created! User created successfully now you can Sign In";                 
                return View("Login");             
            }             
            
            ViewBag.Error = response.Message;
            return View("Login", user);
        }         
       
        catch (MySqlException ex) when (ex.Number == 1062)         
        {             
            ViewBag.Error = "El correo electrónico o el nombre de usuario ya se encuentran registrados.";             
            return View("Login", user);         
        }         
        catch(Exception ex)         
        {             
            ViewBag.Error = $"Explotó el sistema: {ex.Message}";             
            return View("Login", user);         
        }     
    }
    [AllowAnonymous]
    [HttpPost("RegisterOwner")]
    public async Task<IActionResult> RegisterOwner(User user)     
    {         
        var validator = new Validators.UserValidator();
        var validationResult = await validator.ValidateAsync(user);          
        
        if (!validationResult.IsValid)         
        {             
            ViewBag.ErrorValidation = string.Join("<br/>", validationResult.Errors.Select(e => e.ErrorMessage));             
            return View("Login", user);         
        }          
        
        try         
        {             
            var response = await _authService.RegisterOwner(user);             
            if (response.Success)             
            {                 
                ViewBag.Success = "¡Account Created! User created successfully now you can Sign In";                 
                return View("Login");             
            }             
            
            ViewBag.Error = response.Message;
            return View("Login", user);
        }         
       
        catch (MySqlException ex) when (ex.Number == 1062)         
        {             
            ViewBag.Error = "El correo electrónico o el nombre de usuario ya se encuentran registrados.";             
            return View("Login", user);         
        }         
        catch(Exception ex)         
        {             
            ViewBag.Error = $"Explotó el sistema: {ex.Message}";             
            return View("Login", user);         
        }     
    }
    
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Auth");
    }
}
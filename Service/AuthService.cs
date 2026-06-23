using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RentingBooking.Data;
using RentingBooking.Enum;
using RentingBooking.Models;
using RentingBooking.Response;
using RentingBooking.Service.Interfaces;
using RentingBooking.Validators;

namespace RentingBooking.Service;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly UserValidator _userValidator;

    public AuthService(ApplicationDbContext context, IConfiguration configuration,  UserValidator userValidator)
    {
        _context = context;
        _configuration = configuration;
        _userValidator = userValidator;
    }
    
    public async  Task<ServiceResponse<User>> Login(string username, string password)
    {
        var response = new ServiceResponse<User>();
        var userExist = await _context.Users.SingleOrDefaultAsync(u => EF.Functions.Like(u.Username, username));

        if (userExist == null)
        {
            response.Success = false;
            response.Message = "The user you entered does not exist.";
            return response;
        }

        var verificationResult = BCrypt.Net.BCrypt.Verify(password, userExist.PasswordHash);
        if (verificationResult == true)
        {
            var secretKey = _configuration["JwtSettings:SecretKey"];
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.UTF8.GetBytes(secretKey);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            var data = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, userExist.Username),
                    new Claim(ClaimTypes.Role, userExist.Role.ToString()),
                }),
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = credentials,
            };
            var objet = tokenHandler.CreateToken(data);
            var token = tokenHandler.WriteToken(objet); 
            
            userExist.Token = token;
            response.Data = userExist;
            response.Success = true;
            response.Message = "Success loging into the system...";
            
        }
        else
        {
            response.Success = false;
            response.Message = "The user you entered does not exist.";
            return response;
        }
        return response;    
    }

    public async Task<ServiceResponse<User>> RegisterCustomer(User user)
    {
        var response = new ServiceResponse<User>();
        var userValidator = _userValidator.Validate(user);
        
        if (!userValidator.IsValid)
        {
            response.Success = false;
            return response;
        }

        var userExists = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
        if (userExists != null)
        {
            response.Success = false;
            response.Message = "User Already Exists in the system";
            return response;
        }
        else
        {
            user.Role = UserRole.User;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            
            response.Message = "User created successfully";
            response.Success = true;
            response.Data = user;
            
            return response;
        }
    }
    
    public async Task<ServiceResponse<User>> RegisterOwner(User user)
    {
        var response = new ServiceResponse<User>();
        var userValidator = _userValidator.Validate(user);
        
        if (!userValidator.IsValid)
        {
            response.Success = false;
            return response;
        }

        var userExists = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
        if (userExists != null)
        {
            response.Success = false;
            response.Message = "User Already Exists in the system";
            return response;
        }
        else
        {
            user.Role = UserRole.Owner;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            
            response.Message = "User created successfully";
            response.Success = true;
            response.Data = user;
            
            return response;
        }
    }
}
using EStore.API.Data;
using EStore.API.DTOs;
using EStore.API.Models;
using EStore.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EStore.API.Controllers
{
    public class AuthContoller : Controller
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public AuthContoller(AppDbContext context, TokenService tokenService)
        {

            _context = context;
            _tokenService = tokenService;

        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO register)
        {
           if (await _context.Users.AnyAsync(u => u.Email == register.Email))
           {
                return BadRequest("Email already registered.");

           }
              var user = new User
              {
                 Name = register.Name,
                 Email = register.Email,
                 Role = "Customer",
                  PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password)
              };

               _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok("User registered successfully.");

        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto login)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
            if (user == null )
            {
                return Unauthorized("Invalid email or password.");
            }
            bool isValidPassword = BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash);
            if (!isValidPassword)
            {
                return Unauthorized("Invalid email or password.");
            }
            
            var token = _tokenService.CreateToken(user);
            return Ok(new 
            { 
                token = token,
                role=user.Role,
                name=user.Name
            });

        }
    }
}



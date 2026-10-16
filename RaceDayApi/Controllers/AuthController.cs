using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            this._context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.FullName) ||
                    string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Password))
                {
                    return BadRequest("Name, email and password are required.");
                }

                if (dto.Password.Length < 6)
                {
                    return BadRequest("Password must be at least 6 characters.");
                }

                if (dto.Role != Roles.Organiser && dto.Role != Roles.Participant)
                {
                    return BadRequest("Role must be Organiser or Participant.");
                }

                string email = dto.Email.Trim().ToLower();

                bool emailExists = await this._context.Users.AnyAsync(u => u.Email == email);
                if (emailExists)
                {
                    return BadRequest("That email is already registered.");
                }

                User user = new User();
                user.FullName = dto.FullName.Trim();
                user.Email = email;
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                user.Role = dto.Role;
                user.PhoneNumber = dto.PhoneNumber;
                user.DateOfBirth = dto.DateOfBirth;
                user.CreatedDate = DateTime.Now;

                this._context.Users.Add(user);
                await this._context.SaveChangesAsync();

                return StatusCode(201, new
                {
                    userId = user.UserId,
                    fullName = user.FullName,
                    email = user.Email,
                    role = user.Role
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                string email = dto.Email.Trim().ToLower();

                User? user = await this._context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return Unauthorized("Invalid email or password.");
                }

                bool passwordCorrect = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
                if (passwordCorrect == false)
                {
                    return Unauthorized("Invalid email or password.");
                }

                // Save the login in the session
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Role", user.Role);

                return Ok(new
                {
                    userId = user.UserId,
                    fullName = user.FullName,
                    email = user.Email,
                    role = user.Role
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                return Ok("Logged out.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }
    }
}
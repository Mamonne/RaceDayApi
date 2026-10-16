using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [SessionAuthorize]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            this._context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                User? user = await this._context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                return Ok(new
                {
                    userId = user.UserId,
                    fullName = user.FullName,
                    email = user.Email,
                    role = user.Role,
                    phoneNumber = user.PhoneNumber,
                    dateOfBirth = user.DateOfBirth
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile(ProfileUpdateDto dto)
        {
            try
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Email))
                {
                    return BadRequest("Name and email are required.");
                }

                if (dto.Email.Contains("@") == false)
                {
                    return BadRequest("Please enter a valid email address.");
                }

                User? user = await this._context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                string email = dto.Email.Trim().ToLower();

                bool emailTaken = await this._context.Users
                    .AnyAsync(u => u.Email == email && u.UserId != userId);
                if (emailTaken)
                {
                    return BadRequest("That email is already used by another account.");
                }

                user.FullName = dto.FullName.Trim();
                user.Email = email;
                user.PhoneNumber = dto.PhoneNumber;

                await this._context.SaveChangesAsync();

                return Ok(new
                {
                    userId = user.UserId,
                    fullName = user.FullName,
                    email = user.Email,
                    role = user.Role,
                    phoneNumber = user.PhoneNumber
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }
    }
}
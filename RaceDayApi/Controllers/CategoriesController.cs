using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers
{
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            this._context = context;
        }

        // Checks the category details. Returns an error message, or "" if all is fine.
        private string ValidateCategory(CategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CategoryName))
            {
                return "Category name is required.";
            }

            if (dto.Distance <= 0)
            {
                return "Distance must be greater than 0.";
            }

            if (dto.EntryFee < 0)
            {
                return "Entry fee cannot be negative.";
            }

            if (dto.MaxParticipants != null && dto.MaxParticipants <= 0)
            {
                return "Max participants must be greater than 0.";
            }

            return "";
        }

        // GET api/events/5/categories - public
        [HttpGet("api/events/{eventId}/categories")]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            try
            {
                Event? ev = await this._context.Events.FindAsync(eventId);
                if (ev == null)
                {
                    return NotFound("Event not found.");
                }

                List<Category> categories = await this._context.Categories
                    .Where(c => c.EventId == eventId)
                    .OrderBy(c => c.Distance)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        // POST api/events/5/categories - Organiser who owns the event
        [HttpPost("api/events/{eventId}/categories")]
        [SessionAuthorize(Roles.Organiser)]
        public async Task<IActionResult> CreateCategory(int eventId, CategoryDto dto)
        {
            try
            {
                Event? ev = await this._context.Events.FindAsync(eventId);
                if (ev == null)
                {
                    return NotFound("Event not found.");
                }

                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                if (ev.OrganiserId != userId)
                {
                    return StatusCode(403, "You can only add categories to your own events.");
                }

                string error = this.ValidateCategory(dto);
                if (error != "")
                {
                    return BadRequest(error);
                }

                Category category = new Category();
                category.EventId = eventId;
                category.CategoryName = dto.CategoryName.Trim();
                category.Distance = dto.Distance;
                category.EntryFee = dto.EntryFee;
                category.MaxParticipants = dto.MaxParticipants;

                this._context.Categories.Add(category);
                await this._context.SaveChangesAsync();

                return StatusCode(201, category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        // PUT api/categories/5 - Organiser who owns the event
        [HttpPut("api/categories/{id}")]
        [SessionAuthorize(Roles.Organiser)]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDto dto)
        {
            try
            {
                Category? category = await this._context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound("Category not found.");
                }

                Event? ev = await this._context.Events.FindAsync(category.EventId);
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                if (ev == null || ev.OrganiserId != userId)
                {
                    return StatusCode(403, "You can only change categories of your own events.");
                }

                string error = this.ValidateCategory(dto);
                if (error != "")
                {
                    return BadRequest(error);
                }

                category.CategoryName = dto.CategoryName.Trim();
                category.Distance = dto.Distance;
                category.EntryFee = dto.EntryFee;
                category.MaxParticipants = dto.MaxParticipants;

                await this._context.SaveChangesAsync();

                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        // DELETE api/categories/5 - Organiser who owns the event
        [HttpDelete("api/categories/{id}")]
        [SessionAuthorize(Roles.Organiser)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                Category? category = await this._context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound("Category not found.");
                }

                Event? ev = await this._context.Events.FindAsync(category.EventId);
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                if (ev == null || ev.OrganiserId != userId)
                {
                    return StatusCode(403, "You can only delete categories of your own events.");
                }

                // A category that people have enrolled in cannot be deleted
                bool hasEnrolments = await this._context.Enrolments.AnyAsync(e => e.CategoryId == id);
                if (hasEnrolments)
                {
                    return BadRequest("This category has enrolments and cannot be deleted.");
                }

                this._context.Categories.Remove(category);
                await this._context.SaveChangesAsync();

                return Ok("Category deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }
    }
}
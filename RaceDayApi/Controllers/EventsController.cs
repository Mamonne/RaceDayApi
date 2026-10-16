using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventsController(AppDbContext context)
        {
            this._context = context;
        }

        // Turns "running", "RUNNING" etc. into "Running". Returns null if invalid.
        private string? GetValidEventType(string input)
        {
            string value = input.Trim().ToLower();

            if (value == "running")
            {
                return "Running";
            }
            else if (value == "walking")
            {
                return "Walking";
            }
            else if (value == "cycling")
            {
                return "Cycling";
            }
            else
            {
                return null;
            }
        }

        // GET api/events - all upcoming events (public)
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            try
            {
                List<Event> events = await this._context.Events
                    .Where(e => e.EventDate >= DateTime.Today)
                    .OrderBy(e => e.EventDate)
                    .ToListAsync();

                return Ok(events);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        // GET api/events/5 - one event (public)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            try
            {
                Event? ev = await this._context.Events.FindAsync(id);

                if (ev == null)
                {
                    return NotFound("Event not found.");
                }

                return Ok(ev);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        // POST api/events - Organiser only
        [HttpPost]
        [SessionAuthorize(Roles.Organiser)]
        public async Task<IActionResult> CreateEvent(EventDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.EventName) || string.IsNullOrWhiteSpace(dto.Location))
                {
                    return BadRequest("Event name and location are required.");
                }

                string? eventType = this.GetValidEventType(dto.EventType);
                if (eventType == null)
                {
                    return BadRequest("Event type must be Running, Walking or Cycling.");
                }

                if (dto.EventDate.Date < DateTime.Today)
                {
                    return BadRequest("Event date cannot be in the past.");
                }

                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                Event ev = new Event();
                ev.OrganiserId = userId;
                ev.EventName = dto.EventName.Trim();
                ev.EventDate = dto.EventDate;
                ev.Location = dto.Location.Trim();
                ev.Description = dto.Description;
                ev.EventType = eventType;
                ev.CreatedDate = DateTime.Now;

                this._context.Events.Add(ev);
                await this._context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetEvent), new { id = ev.EventId }, ev);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        // PUT api/events/5 - Organiser who owns the event
        [HttpPut("{id}")]
        [SessionAuthorize(Roles.Organiser)]
        public async Task<IActionResult> UpdateEvent(int id, EventDto dto)
        {
            try
            {
                Event? ev = await this._context.Events.FindAsync(id);

                if (ev == null)
                {
                    return NotFound("Event not found.");
                }

                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                if (ev.OrganiserId != userId)
                {
                    return StatusCode(403, "You can only change your own events.");
                }

                if (string.IsNullOrWhiteSpace(dto.EventName) || string.IsNullOrWhiteSpace(dto.Location))
                {
                    return BadRequest("Event name and location are required.");
                }

                string? eventType = this.GetValidEventType(dto.EventType);
                if (eventType == null)
                {
                    return BadRequest("Event type must be Running, Walking or Cycling.");
                }

                ev.EventName = dto.EventName.Trim();
                ev.EventDate = dto.EventDate;
                ev.Location = dto.Location.Trim();
                ev.Description = dto.Description;
                ev.EventType = eventType;

                await this._context.SaveChangesAsync();

                return Ok(ev);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        // DELETE api/events/5 - Organiser who owns the event
        [HttpDelete("{id}")]
        [SessionAuthorize(Roles.Organiser)]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                Event? ev = await this._context.Events.FindAsync(id);

                if (ev == null)
                {
                    return NotFound("Event not found.");
                }

                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                if (ev.OrganiserId != userId)
                {
                    return StatusCode(403, "You can only delete your own events.");
                }

                this._context.Events.Remove(ev);
                await this._context.SaveChangesAsync();

                return Ok("Event deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }
    }
}
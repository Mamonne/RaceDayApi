using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers
{
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoutesController(AppDbContext context)
        {
            this._context = context;
        }

        // GET api/events/5/route - public
        [HttpGet("api/events/{eventId}/route")]
        public async Task<IActionResult> GetRoute(int eventId)
        {
            try
            {
                Event? ev = await this._context.Events.FindAsync(eventId);
                if (ev == null)
                {
                    return NotFound("Event not found.");
                }

                List<RaceRoute> routes = await this._context.Routes
                    .Where(r => r.EventId == eventId)
                    .ToListAsync();

                if (routes.Count == 0)
                {
                    return NotFound("No route has been added for this event.");
                }

                return Ok(routes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        // POST api/events/5/route - Organiser who owns the event
        [HttpPost("api/events/{eventId}/route")]
        [SessionAuthorize(Roles.Organiser)]
        public async Task<IActionResult> AddRoute(int eventId, RouteDto dto)
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
                    return StatusCode(403, "You can only add routes to your own events.");
                }

                if (string.IsNullOrWhiteSpace(dto.RouteName))
                {
                    return BadRequest("Route name is required.");
                }

                if (dto.DistanceKm <= 0)
                {
                    return BadRequest("Distance must be greater than 0.");
                }

                if (dto.ElevationGainM != null && dto.ElevationGainM < 0)
                {
                    return BadRequest("Elevation gain cannot be negative.");
                }

                RaceRoute route = new RaceRoute();
                route.EventId = eventId;
                route.RouteName = dto.RouteName.Trim();
                route.DistanceKm = dto.DistanceKm;
                route.ElevationGainM = dto.ElevationGainM;

                this._context.Routes.Add(route);
                await this._context.SaveChangesAsync();

                return StatusCode(201, route);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }
    }
}
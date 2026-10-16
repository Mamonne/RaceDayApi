using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers
{
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WeatherController(AppDbContext context)
        {
            this._context = context;
        }

        // GET api/events/5/weather - latest forecast (public)
        [HttpGet("api/events/{eventId}/weather")]
        public async Task<IActionResult> GetWeather(int eventId)
        {
            try
            {
                Event? ev = await this._context.Events.FindAsync(eventId);
                if (ev == null)
                {
                    return NotFound("Event not found.");
                }

                Weather? latest = await this._context.Weather
                    .Where(w => w.EventId == eventId)
                    .OrderByDescending(w => w.ForecastDate)
                    .FirstOrDefaultAsync();

                if (latest == null)
                {
                    return NotFound("No weather forecast has been added for this event.");
                }

                return Ok(latest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        // POST api/events/5/weather - Organiser who owns the event
        // Adds a forecast, or updates it if one already exists for that date
        [HttpPost("api/events/{eventId}/weather")]
        [SessionAuthorize(Roles.Organiser)]
        public async Task<IActionResult> AddOrUpdateWeather(int eventId, WeatherDto dto)
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
                    return StatusCode(403, "You can only add weather to your own events.");
                }

                if (dto.ChanceOfRainPercent != null && (dto.ChanceOfRainPercent < 0 || dto.ChanceOfRainPercent > 100))
                {
                    return BadRequest("Chance of rain must be between 0 and 100.");
                }

                if (dto.WindSpeedKmh != null && dto.WindSpeedKmh < 0)
                {
                    return BadRequest("Wind speed cannot be negative.");
                }

                DateTime forecastDate = dto.ForecastDate.Date;

                Weather? existing = await this._context.Weather
                    .FirstOrDefaultAsync(w => w.EventId == eventId && w.ForecastDate == forecastDate);

                if (existing != null)
                {
                    existing.TemperatureC = dto.TemperatureC;
                    existing.WeatherCondition = dto.WeatherCondition;
                    existing.WindSpeedKmh = dto.WindSpeedKmh;
                    existing.ChanceOfRainPercent = dto.ChanceOfRainPercent;

                    await this._context.SaveChangesAsync();
                    return Ok(existing);
                }

                Weather weather = new Weather();
                weather.EventId = eventId;
                weather.ForecastDate = forecastDate;
                weather.TemperatureC = dto.TemperatureC;
                weather.WeatherCondition = dto.WeatherCondition;
                weather.WindSpeedKmh = dto.WindSpeedKmh;
                weather.ChanceOfRainPercent = dto.ChanceOfRainPercent;

                this._context.Weather.Add(weather);
                await this._context.SaveChangesAsync();

                return StatusCode(201, weather);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers
{
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResultsController(AppDbContext context)
        {
            this._context = context;
        }

        // Turns "finished", "dnf" etc. into the correct spelling. Returns null if invalid.
        private string? GetValidStatus(string input)
        {
            string value = input.Trim().ToLower();

            if (value == "finished")
            {
                return "Finished";
            }
            else if (value == "dnf")
            {
                return "DNF";
            }
            else if (value == "dsq")
            {
                return "DSQ";
            }
            else
            {
                return null;
            }
        }

        // POST api/events/5/results - Organiser who owns the event
        [HttpPost("api/events/{eventId}/results")]
        [SessionAuthorize(Roles.Organiser)]
        public async Task<IActionResult> CaptureResult(int eventId, ResultDto dto)
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
                    return StatusCode(403, "You can only capture results for your own events.");
                }

                Enrolment? enrolment = await this._context.Enrolments.FindAsync(dto.EnrolmentId);
                if (enrolment == null || enrolment.EventId != eventId)
                {
                    return BadRequest("That enrolment does not belong to this event.");
                }

                string? status = this.GetValidStatus(dto.Status);
                if (status == null)
                {
                    return BadRequest("Status must be Finished, DNF or DSQ.");
                }

                if (status == "Finished")
                {
                    if (dto.FinishTime == null || dto.FinishTime <= TimeSpan.Zero)
                    {
                        return BadRequest("A finish time is required for finished participants.");
                    }

                    if (dto.OverallPosition == null || dto.OverallPosition <= 0)
                    {
                        return BadRequest("Overall position must be greater than 0.");
                    }
                }

                if (dto.CategoryPosition != null && dto.CategoryPosition <= 0)
                {
                    return BadRequest("Category position must be greater than 0.");
                }

                bool resultExists = await this._context.Results
                    .AnyAsync(r => r.EnrolmentId == dto.EnrolmentId);
                if (resultExists)
                {
                    return BadRequest("A result has already been captured for this enrolment.");
                }

                Result result = new Result();
                result.EnrolmentId = dto.EnrolmentId;
                result.FinishTime = dto.FinishTime;
                result.OverallPosition = dto.OverallPosition;
                result.CategoryPosition = dto.CategoryPosition;
                result.Status = status;

                this._context.Results.Add(result);
                await this._context.SaveChangesAsync();

                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        // GET api/events/5/results - public
        [HttpGet("api/events/{eventId}/results")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            try
            {
                Event? ev = await this._context.Events.FindAsync(eventId);
                if (ev == null)
                {
                    return NotFound("Event not found.");
                }

                var results = await (
                    from r in this._context.Results
                    join en in this._context.Enrolments on r.EnrolmentId equals en.EnrolmentId
                    join u in this._context.Users on en.ParticipantId equals u.UserId
                    join c in this._context.Categories on en.CategoryId equals c.CategoryId
                    where en.EventId == eventId
                    orderby r.OverallPosition
                    select new
                    {
                        resultId = r.ResultId,
                        participantName = u.FullName,
                        categoryName = c.CategoryName,
                        finishTime = r.FinishTime,
                        overallPosition = r.OverallPosition,
                        categoryPosition = r.CategoryPosition,
                        status = r.Status
                    }).ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        // GET api/users/me/results - Participant only (their own results)
        [HttpGet("api/users/me/results")]
        [SessionAuthorize(Roles.Participant)]
        public async Task<IActionResult> GetMyResults()
        {
            try
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                var results = await (
                    from r in this._context.Results
                    join en in this._context.Enrolments on r.EnrolmentId equals en.EnrolmentId
                    join ev in this._context.Events on en.EventId equals ev.EventId
                    join c in this._context.Categories on en.CategoryId equals c.CategoryId
                    where en.ParticipantId == userId
                    orderby ev.EventDate descending
                    select new
                    {
                        resultId = r.ResultId,
                        eventName = ev.EventName,
                        eventDate = ev.EventDate,
                        categoryName = c.CategoryName,
                        finishTime = r.FinishTime,
                        overallPosition = r.OverallPosition,
                        categoryPosition = r.CategoryPosition,
                        status = r.Status
                    }).ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }
    }
}
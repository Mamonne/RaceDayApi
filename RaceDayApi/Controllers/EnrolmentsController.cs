using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;

namespace RaceDayApi.Controllers
{
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EnrolmentsController(AppDbContext context)
        {
            this._context = context;
        }

        // POST api/events/5/enrolments - Participant only
        [HttpPost("api/events/{eventId}/enrolments")]
        [SessionAuthorize(Roles.Participant)]
        public async Task<IActionResult> Enrol(int eventId, EnrolmentDto dto)
        {
            try
            {
                Event? ev = await this._context.Events.FindAsync(eventId);
                if (ev == null)
                {
                    return NotFound("Event not found.");
                }

                if (ev.EventDate.Date < DateTime.Today)
                {
                    return BadRequest("This event has already taken place.");
                }

                Category? category = await this._context.Categories.FindAsync(dto.CategoryId);
                if (category == null || category.EventId != eventId)
                {
                    return BadRequest("That category does not belong to this event.");
                }

                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                // A participant can only enter an event once
                bool alreadyEnrolled = await this._context.Enrolments
                    .AnyAsync(e => e.EventId == eventId && e.ParticipantId == userId);
                if (alreadyEnrolled)
                {
                    return BadRequest("You are already enrolled in this event.");
                }

                // Check that the category is not full
                if (category.MaxParticipants != null)
                {
                    int enrolledCount = await this._context.Enrolments
                        .CountAsync(e => e.CategoryId == dto.CategoryId);

                    if (enrolledCount >= category.MaxParticipants)
                    {
                        return BadRequest("This category is full.");
                    }
                }

                Enrolment enrolment = new Enrolment();
                enrolment.ParticipantId = userId;
                enrolment.EventId = eventId;
                enrolment.CategoryId = dto.CategoryId;
                enrolment.RegistrationDate = DateTime.Now;
                enrolment.PaymentStatus = "Pending";

                this._context.Enrolments.Add(enrolment);
                await this._context.SaveChangesAsync();

                return StatusCode(201, enrolment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        // GET api/events/5/enrolments - Organiser who owns the event
        [HttpGet("api/events/{eventId}/enrolments")]
        [SessionAuthorize(Roles.Organiser)]
        public async Task<IActionResult> GetEnrolments(int eventId)
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
                    return StatusCode(403, "You can only view enrolments for your own events.");
                }

                // Join enrolments with users and categories to show names
                var enrolments = await (
                    from en in this._context.Enrolments
                    join u in this._context.Users on en.ParticipantId equals u.UserId
                    join c in this._context.Categories on en.CategoryId equals c.CategoryId
                    where en.EventId == eventId
                    select new
                    {
                        enrolmentId = en.EnrolmentId,
                        participantId = u.UserId,
                        participantName = u.FullName,
                        categoryId = c.CategoryId,
                        categoryName = c.CategoryName,
                        registrationDate = en.RegistrationDate,
                        paymentStatus = en.PaymentStatus
                    }).ToListAsync();

                return Ok(enrolments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }

        // DELETE api/enrolments/5 - Participant who owns the enrolment
        [HttpDelete("api/enrolments/{id}")]
        [SessionAuthorize(Roles.Participant)]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            try
            {
                Enrolment? enrolment = await this._context.Enrolments.FindAsync(id);
                if (enrolment == null)
                {
                    return NotFound("Enrolment not found.");
                }

                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                if (enrolment.ParticipantId != userId)
                {
                    return StatusCode(403, "You can only cancel your own enrolment.");
                }

                // Cannot cancel once a result has been captured
                bool hasResult = await this._context.Results.AnyAsync(r => r.EnrolmentId == id);
                if (hasResult)
                {
                    return BadRequest("A result has already been captured, so this enrolment cannot be cancelled.");
                }

                this._context.Enrolments.Remove(enrolment);
                await this._context.SaveChangesAsync();

                return Ok("Enrolment cancelled.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong: " + ex.Message);
            }
        }
    }
}

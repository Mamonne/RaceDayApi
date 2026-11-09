using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Filters;
using RaceDayMvc.Models;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    [SessionAuthorize(Roles.Organiser)]
    public class EventResultsController : Controller
    {
        private readonly ApiService _api;

        public EventResultsController(ApiService api)
        {
            this._api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int eventId)
        {
            try
            {
                ApiResponse eventResponse = await this._api.GetAsync("api/events/" + eventId);
                if (eventResponse.Success == false)
                {
                    TempData["Error"] = eventResponse.Message;
                    return RedirectToAction("Dashboard", "Organiser");
                }

                EventItem? ev = eventResponse.ReadAs<EventItem>();
                if (ev == null)
                {
                    TempData["Error"] = "Something went wrong. Please try again.";
                    return RedirectToAction("Dashboard", "Organiser");
                }

                // The API only lets the organiser who owns the event see its enrolments
                ApiResponse enrolmentResponse = await this._api.GetAsync("api/events/" + eventId + "/enrolments");
                if (enrolmentResponse.Success == false)
                {
                    TempData["Error"] = enrolmentResponse.Message;
                    return RedirectToAction("Dashboard", "Organiser");
                }

                List<EventEnrolmentItem> enrolments = enrolmentResponse.ReadAs<List<EventEnrolmentItem>>()
                    ?? new List<EventEnrolmentItem>();

                ApiResponse resultResponse = await this._api.GetAsync("api/events/" + eventId + "/results");
                List<EventResultItem> results = new List<EventResultItem>();
                if (resultResponse.Success)
                {
                    results = resultResponse.ReadAs<List<EventResultItem>>() ?? new List<EventResultItem>();
                }

                EventResultsViewModel model = new EventResultsViewModel();
                model.EventId = ev.EventId;
                model.EventName = ev.EventName;

                foreach (EventEnrolmentItem enrolment in enrolments)
                {
                    // Cancelled enrolments do not get results
                    if (enrolment.PaymentStatus == "Cancelled")
                    {
                        continue;
                    }

                    ResultRow row = new ResultRow();
                    row.Enrolment = enrolment;
                    row.Result = results.FirstOrDefault(r => r.EnrolmentId == enrolment.EnrolmentId);
                    model.Rows.Add(row);
                }

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong. Please try again.";
                return RedirectToAction("Dashboard", "Organiser");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Capture(int eventId, int enrolmentId)
        {
            try
            {
                string? name = await this.GetParticipantName(eventId, enrolmentId);
                if (name == null)
                {
                    TempData["Error"] = "We could not find that enrolment.";
                    return RedirectToAction("Index", new { eventId = eventId });
                }

                ResultFormViewModel model = new ResultFormViewModel();
                model.EventId = eventId;
                model.EnrolmentId = enrolmentId;

                ViewData["ParticipantName"] = name;
                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong. Please try again.";
                return RedirectToAction("Index", new { eventId = eventId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Capture(ResultFormViewModel model)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    ViewData["ParticipantName"] = await this.GetParticipantName(model.EventId, model.EnrolmentId);
                    return View(model);
                }

                // The time is typed as text (hh:mm:ss), so turn it into a time value
                TimeSpan? finishTime = null;
                if (string.IsNullOrWhiteSpace(model.FinishTime) == false)
                {
                    finishTime = TimeSpan.Parse(model.FinishTime, System.Globalization.CultureInfo.InvariantCulture);
                }

                var body = new
                {
                    enrolmentId = model.EnrolmentId,
                    finishTime = finishTime,
                    overallPosition = model.OverallPosition,
                    categoryPosition = model.CategoryPosition,
                    status = model.Status
                };

                // The API checks the rules (for example, a finisher needs a time) and sends a message if not
                ApiResponse response = await this._api.PostAsync("api/events/" + model.EventId + "/results", body);

                if (response.Success == false)
                {
                    ModelState.AddModelError("", response.Message);
                    ViewData["ParticipantName"] = await this.GetParticipantName(model.EventId, model.EnrolmentId);
                    return View(model);
                }

                TempData["Success"] = "The result was saved and is now visible to the participant.";
                return RedirectToAction("Index", new { eventId = model.EventId });
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Something went wrong. Please try again.");
                return View(model);
            }
        }

        // Finds the participant's name for an enrolment of this event
        private async Task<string?> GetParticipantName(int eventId, int enrolmentId)
        {
            ApiResponse response = await this._api.GetAsync("api/events/" + eventId + "/enrolments");
            if (response.Success == false)
            {
                return null;
            }

            List<EventEnrolmentItem>? list = response.ReadAs<List<EventEnrolmentItem>>();
            EventEnrolmentItem? item = list?.FirstOrDefault(e => e.EnrolmentId == enrolmentId);

            return item?.ParticipantName;
        }
    }
}
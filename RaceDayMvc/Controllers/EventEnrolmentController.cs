using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Filters;
using RaceDayMvc.Models;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    [SessionAuthorize(Roles.Organiser)]
    public class EventEnrolmentsController : Controller
    {
        private readonly ApiService _api;

        public EventEnrolmentsController(ApiService api)
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
                ApiResponse response = await this._api.GetAsync("api/events/" + eventId + "/enrolments");
                if (response.Success == false)
                {
                    TempData["Error"] = response.Message;
                    return RedirectToAction("Dashboard", "Organiser");
                }

                EventEnrolmentsViewModel model = new EventEnrolmentsViewModel();
                model.EventId = ev.EventId;
                model.EventName = ev.EventName;

                List<EventEnrolmentItem>? items = response.ReadAs<List<EventEnrolmentItem>>();
                if (items != null)
                {
                    model.Enrolments = items;
                }

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong. Please try again.";
                return RedirectToAction("Dashboard", "Organiser");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int eventId, int id, string status)
        {
            try
            {
                var body = new { paymentStatus = status };
                ApiResponse response = await this._api.PutAsync("api/enrolments/" + id + "/status", body);

                if (response.Success)
                {
                    TempData["Success"] = "The enrolment was updated.";
                }
                else
                {
                    TempData["Error"] = response.Message;
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong. Please try again.";
            }

            return RedirectToAction("Index", new { eventId = eventId });
        }
    }
}
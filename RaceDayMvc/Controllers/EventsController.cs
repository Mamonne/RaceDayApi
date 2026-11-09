using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Filters;
using RaceDayMvc.Models;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApiService _api;

        public EventsController(ApiService api)
        {
            this._api = api;
        }

        // Anyone can look at an event
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            EventDetailsViewModel model = new EventDetailsViewModel();

            try
            {
                ApiResponse eventResponse = await this._api.GetAsync("api/events/" + id);

                if (eventResponse.Success == false)
                {
                    model.ErrorMessage = eventResponse.Message;
                    return View(model);
                }

                model.Event = eventResponse.ReadAs<EventItem>();

                ApiResponse categoryResponse = await this._api.GetAsync("api/events/" + id + "/categories");
                if (categoryResponse.Success)
                {
                    List<CategoryItem>? categories = categoryResponse.ReadAs<List<CategoryItem>>();
                    if (categories != null)
                    {
                        model.Categories = categories;
                    }
                }
            }
            catch (Exception)
            {
                model.ErrorMessage = "Something went wrong. Please try again.";
            }

            return View(model);
        }

        // Only a logged-in Participant can enrol
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles.Participant)]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            try
            {
                var body = new { categoryId = categoryId };
                ApiResponse response = await this._api.PostAsync("api/events/" + eventId + "/enrolments", body);

                if (response.Success)
                {
                    TempData["Success"] = "You are enrolled! Your enrolment is pending until it is confirmed.";
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

            return RedirectToAction("Details", new { id = eventId });
        }
    }
}
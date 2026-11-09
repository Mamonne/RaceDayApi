using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Filters;
using RaceDayMvc.Models;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    [SessionAuthorize(Roles.Organiser)]
    public class ManageEventsController : Controller
    {
        private readonly ApiService _api;

        public ManageEventsController(ApiService api)
        {
            this._api = api;
        }

        // The details we send to the API when creating or updating an event
        private object MakeBody(EventFormViewModel model)
        {
            return new
            {
                eventName = model.EventName.Trim(),
                eventDate = model.EventDate,
                location = model.Location.Trim(),
                description = model.Description,
                distance = model.Distance,
                eventType = model.EventType
            };
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new EventFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventFormViewModel model)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return View(model);
                }

                ApiResponse response = await this._api.PostAsync("api/events", this.MakeBody(model));

                if (response.Success == false)
                {
                    ModelState.AddModelError("", response.Message);
                    return View(model);
                }

                TempData["Success"] = "The event was created.";
                return RedirectToAction("Dashboard", "Organiser");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Something went wrong. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                ApiResponse response = await this._api.GetAsync("api/events/" + id);

                if (response.Success == false)
                {
                    TempData["Error"] = response.Message;
                    return RedirectToAction("Dashboard", "Organiser");
                }

                EventItem? item = response.ReadAs<EventItem>();
                if (item == null)
                {
                    TempData["Error"] = "Something went wrong. Please try again.";
                    return RedirectToAction("Dashboard", "Organiser");
                }

                // An organiser can only edit their own events
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                if (item.OrganiserId != userId)
                {
                    TempData["Error"] = "You can only change your own events.";
                    return RedirectToAction("Dashboard", "Organiser");
                }

                EventFormViewModel model = new EventFormViewModel();
                model.EventId = item.EventId;
                model.EventName = item.EventName;
                model.EventDate = item.EventDate;
                model.Location = item.Location;
                model.Description = item.Description;
                model.Distance = item.Distance;
                model.EventType = item.EventType;

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
        public async Task<IActionResult> Edit(EventFormViewModel model)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return View(model);
                }

                ApiResponse response = await this._api.PutAsync("api/events/" + model.EventId, this.MakeBody(model));

                if (response.Success == false)
                {
                    ModelState.AddModelError("", response.Message);
                    return View(model);
                }

                TempData["Success"] = "The event was updated.";
                return RedirectToAction("Dashboard", "Organiser");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Something went wrong. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                ApiResponse response = await this._api.DeleteAsync("api/events/" + id);

                if (response.Success)
                {
                    TempData["Success"] = "The event was deleted.";
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

            return RedirectToAction("Dashboard", "Organiser");
        }
    }
}
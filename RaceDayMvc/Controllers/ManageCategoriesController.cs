using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Filters;
using RaceDayMvc.Models;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    [SessionAuthorize(Roles.Organiser)]
    public class ManageCategoriesController : Controller
    {
        private readonly ApiService _api;

        public ManageCategoriesController(ApiService api)
        {
            this._api = api;
        }

        // Gets the event, but only if it belongs to the logged-in organiser.
        // Returns null (and sets an error message) if not.
        private async Task<EventItem?> GetOwnedEvent(int eventId)
        {
            ApiResponse response = await this._api.GetAsync("api/events/" + eventId);

            if (response.Success == false)
            {
                TempData["Error"] = response.Message;
                return null;
            }

            EventItem? item = response.ReadAs<EventItem>();
            if (item == null)
            {
                TempData["Error"] = "Something went wrong. Please try again.";
                return null;
            }

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (item.OrganiserId != userId)
            {
                TempData["Error"] = "You can only manage categories for your own events.";
                return null;
            }

            return item;
        }

        // Gets the categories of an event
        private async Task<List<CategoryItem>> GetCategories(int eventId)
        {
            ApiResponse response = await this._api.GetAsync("api/events/" + eventId + "/categories");

            if (response.Success)
            {
                List<CategoryItem>? list = response.ReadAs<List<CategoryItem>>();
                if (list != null)
                {
                    return list;
                }
            }

            return new List<CategoryItem>();
        }

        private object MakeBody(CategoryFormViewModel model)
        {
            return new
            {
                categoryName = model.CategoryName.Trim(),
                distance = model.Distance,
                entryFee = model.EntryFee,
                maxParticipants = model.MaxParticipants
            };
        }

        [HttpGet]
        public async Task<IActionResult> Index(int eventId)
        {
            try
            {
                EventItem? ev = await this.GetOwnedEvent(eventId);
                if (ev == null)
                {
                    return RedirectToAction("Dashboard", "Organiser");
                }

                ManageCategoriesViewModel model = new ManageCategoriesViewModel();
                model.EventId = ev.EventId;
                model.EventName = ev.EventName;
                model.Categories = await this.GetCategories(eventId);

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong. Please try again.";
                return RedirectToAction("Dashboard", "Organiser");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(int eventId)
        {
            try
            {
                EventItem? ev = await this.GetOwnedEvent(eventId);
                if (ev == null)
                {
                    return RedirectToAction("Dashboard", "Organiser");
                }

                CategoryFormViewModel model = new CategoryFormViewModel();
                model.EventId = eventId;

                ViewData["EventName"] = ev.EventName;
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
        public async Task<IActionResult> Create(CategoryFormViewModel model)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return View(model);
                }

                ApiResponse response = await this._api.PostAsync(
                    "api/events/" + model.EventId + "/categories", this.MakeBody(model));

                if (response.Success == false)
                {
                    ModelState.AddModelError("", response.Message);
                    return View(model);
                }

                TempData["Success"] = "The category was added.";
                return RedirectToAction("Index", new { eventId = model.EventId });
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Something went wrong. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int eventId, int id)
        {
            try
            {
                EventItem? ev = await this.GetOwnedEvent(eventId);
                if (ev == null)
                {
                    return RedirectToAction("Dashboard", "Organiser");
                }

                List<CategoryItem> categories = await this.GetCategories(eventId);
                CategoryItem? category = categories.FirstOrDefault(c => c.CategoryId == id);

                if (category == null)
                {
                    TempData["Error"] = "We could not find that category.";
                    return RedirectToAction("Index", new { eventId = eventId });
                }

                CategoryFormViewModel model = new CategoryFormViewModel();
                model.CategoryId = category.CategoryId;
                model.EventId = category.EventId;
                model.CategoryName = category.CategoryName;
                model.Distance = category.Distance;
                model.EntryFee = category.EntryFee;
                model.MaxParticipants = category.MaxParticipants;

                ViewData["EventName"] = ev.EventName;
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
        public async Task<IActionResult> Edit(CategoryFormViewModel model)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return View(model);
                }

                ApiResponse response = await this._api.PutAsync(
                    "api/categories/" + model.CategoryId, this.MakeBody(model));

                if (response.Success == false)
                {
                    ModelState.AddModelError("", response.Message);
                    return View(model);
                }

                TempData["Success"] = "The category was updated.";
                return RedirectToAction("Index", new { eventId = model.EventId });
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Something went wrong. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int eventId, int id)
        {
            try
            {
                ApiResponse response = await this._api.DeleteAsync("api/categories/" + id);

                if (response.Success)
                {
                    TempData["Success"] = "The category was deleted.";
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
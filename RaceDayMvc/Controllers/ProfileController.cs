using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Filters;
using RaceDayMvc.Models;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    // Any logged-in user (Organiser or Participant) can use this page
    [SessionAuthorize]
    public class ProfileController : Controller
    {
        private readonly ApiService _api;

        public ProfileController(ApiService api)
        {
            this._api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                ApiResponse response = await this._api.GetAsync("api/profile");

                if (response.Success == false)
                {
                    TempData["Error"] = response.Message;
                    return RedirectToAction("Index", "Home");
                }

                ProfileViewModel? model = response.ReadAs<ProfileViewModel>();
                if (model == null)
                {
                    TempData["Error"] = "Something went wrong. Please try again.";
                    return RedirectToAction("Index", "Home");
                }

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            try
            {
                // If a field is not valid, show the first message and go back to the profile page
                if (ModelState.IsValid == false)
                {
                    string message = "Please check your details and try again.";

                    foreach (var entry in ModelState.Values)
                    {
                        if (entry.Errors.Count > 0)
                        {
                            message = entry.Errors[0].ErrorMessage;
                            break;
                        }
                    }

                    TempData["Error"] = message;
                    return RedirectToAction("Index");
                }

                var body = new
                {
                    fullName = model.FullName,
                    email = model.Email,
                    phoneNumber = model.PhoneNumber ?? ""
                };

                ApiResponse response = await this._api.PutAsync("api/profile", body);

                if (response.Success)
                {
                    // Keep the name in the menu up to date
                    HttpContext.Session.SetString("FullName", model.FullName.Trim());
                    TempData["Success"] = "Your profile was updated.";
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

            return RedirectToAction("Index");
        }
    }
}
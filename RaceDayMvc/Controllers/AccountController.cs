using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Models;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _api;

        public AccountController(ApiService api)
        {
            this._api = api;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return View(model);
                }

                var body = new { email = model.Email, password = model.Password };
                ApiResponse response = await this._api.PostAsync("api/auth/login", body);

                if (response.Success == false)
                {
                    if (response.StatusCode == 401)
                    {
                        ModelState.AddModelError("", "Incorrect email or password.");
                    }
                    else
                    {
                        ModelState.AddModelError("", response.Message);
                    }

                    return View(model);
                }

                LoginResult? user = response.ReadAs<LoginResult>();
                if (user == null)
                {
                    ModelState.AddModelError("", "Something went wrong. Please try again.");
                    return View(model);
                }

                // Remember who is logged in (the API cookie was saved by ApiService)
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("Role", user.Role);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Something went wrong. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return View(model);
                }

                var body = new
                {
                    fullName = model.FullName,
                    email = model.Email,
                    password = model.Password,
                    role = model.Role,
                    phoneNumber = model.PhoneNumber,
                    dateOfBirth = model.DateOfBirth
                };

                ApiResponse response = await this._api.PostAsync("api/auth/register", body);

                if (response.Success == false)
                {
                    ModelState.AddModelError("", response.Message);
                    return View(model);
                }

                TempData["Success"] = "Your account was created. Please log in.";
                return RedirectToAction("Login");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Something went wrong. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await this._api.PostAsync("api/auth/logout", null);
            }
            catch (Exception)
            {
                // Even if the API cannot be reached, we still end the MVC session below
            }

            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out.";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
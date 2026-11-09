using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Models;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _api;

        public HomeController(ApiService api)
        {
            this._api = api;
        }

        public async Task<IActionResult> Index(string? search, string? eventType, decimal? minDistance, decimal? maxDistance)
        {
            // Organisers have their own home page: the dashboard
            string? role = HttpContext.Session.GetString("Role");
            if (role == Roles.Organiser)
            {
                return RedirectToAction("Dashboard", "Organiser");
            }

            HomeIndexViewModel model = new HomeIndexViewModel();
            model.Search = search;
            model.EventType = eventType;
            model.MinDistance = minDistance;
            model.MaxDistance = maxDistance;

            try
            {
                // Build the filter part of the API address
                List<string> parts = new List<string>();

                if (string.IsNullOrWhiteSpace(search) == false)
                {
                    parts.Add("search=" + Uri.EscapeDataString(search.Trim()));
                }

                if (string.IsNullOrWhiteSpace(eventType) == false)
                {
                    parts.Add("eventType=" + Uri.EscapeDataString(eventType));
                }

                if (minDistance != null)
                {
                    parts.Add("minDistance=" + minDistance.Value.ToString(CultureInfo.InvariantCulture));
                }

                if (maxDistance != null)
                {
                    parts.Add("maxDistance=" + maxDistance.Value.ToString(CultureInfo.InvariantCulture));
                }

                string url = "api/events";
                if (parts.Count > 0)
                {
                    url = url + "?" + string.Join("&", parts);
                }

                ApiResponse response = await this._api.GetAsync(url);

                if (response.Success)
                {
                    List<EventItem>? events = response.ReadAs<List<EventItem>>();
                    if (events != null)
                    {
                        model.Events = events;
                    }
                }
                else
                {
                    model.ErrorMessage = response.Message;
                }
            }
            catch (Exception)
            {
                model.ErrorMessage = "Something went wrong. Please try again.";
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
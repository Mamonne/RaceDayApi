using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Filters;
using RaceDayMvc.Models;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    [SessionAuthorize(Roles.Organiser)]
    public class OrganiserController : Controller
    {
        private readonly ApiService _api;

        public OrganiserController(ApiService api)
        {
            this._api = api;
        }

        public async Task<IActionResult> Dashboard()
        {
            DashboardViewModel model = new DashboardViewModel();

            try
            {
                ApiResponse response = await this._api.GetAsync("api/events/mine");

                if (response.Success)
                {
                    List<MyEventItem>? items = response.ReadAs<List<MyEventItem>>();

                    if (items != null)
                    {
                        model.Events = items;
                        model.TotalEvents = items.Count;

                        foreach (MyEventItem item in items)
                        {
                            model.TotalEnrolments = model.TotalEnrolments + item.TotalEnrolments;

                            if (item.EventDate.Date >= DateTime.Today)
                            {
                                model.UpcomingEvents = model.UpcomingEvents + 1;

                                // The events come sorted by date, so the first upcoming one is the next event
                                if (model.NextEventDate == null)
                                {
                                    model.NextEventDate = item.EventDate;
                                }
                            }
                        }
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
    }
}
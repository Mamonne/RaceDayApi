using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Filters;
using RaceDayMvc.Models;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    [SessionAuthorize(Roles.Participant)]
    public class ParticipantController : Controller
    {
        private readonly ApiService _api;

        public ParticipantController(ApiService api)
        {
            this._api = api;
        }

        public async Task<IActionResult> MyEnrolments()
        {
            MyEnrolmentsViewModel model = new MyEnrolmentsViewModel();

            try
            {
                ApiResponse response = await this._api.GetAsync("api/users/me/enrolments");

                if (response.Success)
                {
                    List<EnrolmentItem>? items = response.ReadAs<List<EnrolmentItem>>();
                    if (items != null)
                    {
                        model.Enrolments = items;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                ApiResponse response = await this._api.DeleteAsync("api/enrolments/" + id);

                if (response.Success)
                {
                    TempData["Success"] = "Your enrolment was cancelled.";
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

            return RedirectToAction("MyEnrolments");
        }

        public async Task<IActionResult> MyResults()
        {
            MyResultsViewModel model = new MyResultsViewModel();

            try
            {
                ApiResponse response = await this._api.GetAsync("api/users/me/results");

                if (response.Success)
                {
                    List<ResultItem>? items = response.ReadAs<List<ResultItem>>();
                    if (items != null)
                    {
                        model.Results = items;
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
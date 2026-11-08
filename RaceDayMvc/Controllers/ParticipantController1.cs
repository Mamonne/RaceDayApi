using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Filters;
using RaceDayMvc.Models;

namespace RaceDayMvc.Controllers
{
    [SessionAuthorize(Roles.Participant)]
    public class ParticipantController : Controller
    {
        public IActionResult MyEnrolments()
        {
            return View();
        }
    }
}
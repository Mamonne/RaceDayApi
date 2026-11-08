using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Filters;
using RaceDayMvc.Models;

namespace RaceDayMvc.Controllers
{
    [SessionAuthorize(Roles.Organiser)]
    public class OrganiserController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
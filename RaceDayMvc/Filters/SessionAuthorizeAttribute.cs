using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDayMvc.Filters
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string _role;

        public SessionAuthorizeAttribute(string role = "")
        {
            this._role = role;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                int? userId = context.HttpContext.Session.GetInt32("UserId");
                string? userRole = context.HttpContext.Session.GetString("Role");

                // Not logged in: go to the login page
                if (userId == null)
                {
                    context.Result = new RedirectToActionResult("Login", "Account", null);
                    return;
                }

                // Logged in, but the wrong role: go to the access denied page
                if (this._role != "" && userRole != this._role)
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                }
            }
            catch (Exception)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }
    }
}
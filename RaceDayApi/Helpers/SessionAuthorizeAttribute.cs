using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDayApi.Helpers
{
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _role;

        public SessionAuthorizeAttribute(string role = "")
        {
            this._role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                int? userId = context.HttpContext.Session.GetInt32("UserId");
                string? userRole = context.HttpContext.Session.GetString("Role");

                // Not logged in
                if (userId == null)
                {
                    context.Result = new UnauthorizedObjectResult("You must be logged in.");
                    return;
                }

                // Logged in, but the wrong role
                if (this._role != "" && userRole != this._role)
                {
                    context.Result = new ObjectResult("You do not have permission to do this.")
                    {
                        StatusCode = 403
                    };
                }
            }
            catch (Exception ex)
            {
                context.Result = new ObjectResult("Something went wrong: " + ex.Message)
                {
                    StatusCode = 500
                };
            }
        }
    }
}
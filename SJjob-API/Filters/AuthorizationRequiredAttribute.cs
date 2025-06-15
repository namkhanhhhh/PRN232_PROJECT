using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SJOB_EXE201.Filters
{
    public class AuthorizationRequiredAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if the user is authenticated
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                // Set a flag in TempData to show the auth notification
                context.HttpContext.Response.Cookies.Append("ShowAuthNotification", "true", new CookieOptions
                {
                    Path = "/",
                    HttpOnly = false,
                    Expires = DateTimeOffset.Now.AddMinutes(1)
                });

                // Redirect to the requested URL (it will show the notification)
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectResult($"/Worker/Index?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }
        }
    }
}

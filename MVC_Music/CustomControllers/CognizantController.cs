using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MVC_Music.Utilities;

namespace MVC_Music.CustomControllers
{
    /// <summary>
    /// Makes the controller "self aware" knowing it's own name
    /// and what Action was called.
    /// </summary>
    public class CognizantController : Controller
    {
        internal string ControllerName()
        {
            return ControllerContext.RouteData.Values["controller"]?.ToString() ?? string.Empty;
        }

        internal string ActionName()
        {
            return ControllerContext.RouteData.Values["action"]?.ToString() ?? string.Empty;
        }

        internal static string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex
                .Replace(input, "([A-Z])", " $1",
                System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ApplyViewData();
            base.OnActionExecuting(context);
        }

        private void ApplyViewData()
        {
            string controllerFriendlyName = SplitCamelCase(ControllerName());
            ViewData["ControllerName"] = ControllerName();
            ViewData["ControllerFriendlyName"] = controllerFriendlyName;
            ViewData["ActionName"] = ActionName();
            ViewData["ActionNameLowerCase"] = ActionName().ToLower();
            ViewData["Title"] = controllerFriendlyName + " " + ActionName();
        }

        /// <summary>
        /// Redirects only to validated local paths for the given controller; otherwise Index.
        /// </summary>
        protected IActionResult SafeRedirectToReturnUrl(string? url, string? fallbackController = null)
        {
            fallbackController ??= ControllerName();
            if (MaintainURL.IsValidLocalReturnUrl(url, fallbackController))
            {
                return LocalRedirect(url!);
            }

            return RedirectToAction("Index", fallbackController);
        }
    }
}

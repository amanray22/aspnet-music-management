using Microsoft.AspNetCore.Mvc.Filters;
using MVC_Music.Utilities;

namespace MVC_Music.CustomControllers
{
    /// <summary>
    /// Persists Index sort, filter, and paging parameters via cookies and ViewData.
    /// </summary>
    public class ElephantController : CognizantController
    {
        internal string[] ActionWithURL =
        [
            "Details", "Create", "Edit", "Delete", "Add", "Update", "Remove"
        ];

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (ActionWithURL.Contains(ActionName()))
            {
                ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
            }
            else if (ActionName() == "Index")
            {
                CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);
            }

            base.OnActionExecuting(context);
        }
    }
}

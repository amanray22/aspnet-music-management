namespace MVC_Music.Utilities
{
    public static class MaintainURL
    {
        /// <summary>
        /// Maintain the URL for an Index View including filter, sort and page information.
        /// Depends on the CookieHelper Utility.
        /// </summary>
        public static string ReturnURL(HttpContext httpContext, string controllerName)
        {
            string cookieName = controllerName + "URL";
            string searchText = "/" + controllerName + "?";
            string fallback = "/" + controllerName;

            string returnURL = httpContext.Request.Headers.Referer.ToString();
            if (returnURL.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                returnURL = returnURL[returnURL.LastIndexOf(searchText, StringComparison.OrdinalIgnoreCase)..];
                if (IsValidLocalReturnUrl(returnURL, controllerName))
                {
                    CookieHelper.CookieSet(httpContext, cookieName, returnURL, 30);
                    return returnURL;
                }
            }

            returnURL = httpContext.Request.Cookies[cookieName] ?? fallback;
            return IsValidLocalReturnUrl(returnURL, controllerName) ? returnURL : fallback;
        }

        /// <summary>
        /// Ensures return URLs are relative application paths for the expected controller only.
        /// </summary>
        public static bool IsValidLocalReturnUrl(string? url, string controllerName)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            if (!url.StartsWith('/') || url.StartsWith("//", StringComparison.Ordinal))
            {
                return false;
            }

            if (url.Contains("://", StringComparison.OrdinalIgnoreCase)
                || url.Contains('\\'))
            {
                return false;
            }

            var expectedPrefix = "/" + controllerName;
            return url.Equals(expectedPrefix, StringComparison.OrdinalIgnoreCase)
                || url.StartsWith(expectedPrefix + "?", StringComparison.OrdinalIgnoreCase);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
// using System.Web.Http.Controllers;
// using System.Web.Http.Filters;

namespace ArabicTextAnalyzer.BO
{
    /*public class AuthenticateFilterAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Processes HTTP requests that fail authorization.
        /// </summary>
        /// <param name="filterContext">Encapsulates the information for using <see cref="T:System.Web.Mvc.AuthorizeAttribute"/>. The <paramref name="filterContext"/> object contains the controller, HTTP context, request context, action result, and route data.</param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // handle tell ajax it is 401 unauthrized instead of redirect (thus parseing error) when sessin timeout
            if (filterContext.HttpContext.Request.IsAjaxRequest())
                filterContext.HttpContext.Items["AjaxPermissionDenied"] = true;

            base.HandleUnauthorizedRequest(filterContext);
        }
    }*/
}
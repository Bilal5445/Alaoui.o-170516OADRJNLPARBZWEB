using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ArabicTextAnalyzer
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // for localization of cshtml
            /*routes.MapRoute(
                name: "LocalizedDefault",
                url: "{lang}/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new { lang = "fr-FR|en-US" }
            );*/

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, lang = "fr-FR" }
            );

            // for localization of js
            routes.MapRoute(
                name: "Resources", 
                url: "Scripts/mysite_train_keywordfiltering.js", 
                defaults: new { controller = "Base2", action = "GetResourcesJavaScript" }
            );
            /*routes.ma.MapRouteWithName(
           "DataSourceJS", // Route name
           "Scripts/Entities/{controller}/datasource.js", // URL with parameters
           new { controller = "Home", action = "DataSourceJS" } // Parameter defaults,
           , null
           );*/
        }
    }
}

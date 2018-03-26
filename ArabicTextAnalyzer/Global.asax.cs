using System.Globalization;
using System.Threading;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ArabicTextAnalyzer
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // limit response to json only
            ConfigureApi(GlobalConfiguration.Configuration);
        }

        protected void Application_BeginRequest()
        {
            // CultureInfo culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            // CultureInfo culture = new System.Globalization.CultureInfo("fr");
            // culture.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
            // culture.DateTimeFormat.LongTimePattern = "";
            // Thread.CurrentThread.CurrentCulture = culture;
        }

        void ConfigureApi(HttpConfiguration config)
        {
            // Remove the JSON formatter
            // config.Formatters.Remove(config.Formatters.JsonFormatter);

            // or

            // Remove the XML formatter
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}

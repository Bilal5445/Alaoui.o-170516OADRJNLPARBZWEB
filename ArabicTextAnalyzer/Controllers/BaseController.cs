using ArabicTextAnalyzer.Content.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ArabicTextAnalyzer.Controllers
{
    public class Base2Controller : Controller
    {
        //
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();
        public ActionResult GetResourcesJavaScript(/*string resxFileName*/)
        {
            throw new Exception("mlklk");
            ResourceSet resourceSet = R.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            var resourceDictionary = resourceSet
                                .Cast<DictionaryEntry>()
                                .ToDictionary(entry => entry.Key.ToString(), entry => entry.Value.ToString());
            var json = Serializer.Serialize(resourceDictionary);
            // var javaScript = string.Format("window.Resources = window.Resources || {{}}; window.Resources.{0} = {1};", resxFileName, json);

            // return JavaScript(javaScript);
            return JavaScript(json);
            // return JavaScript("");
            // return Content("var InitializeDataTables = \"Hello World!\";");
        }
    }

    public class BaseController : Controller
    {
        private static string _cookieLangName = "Lang4MultiLangSupport";

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string cultureOnCookie = GetCultureOnCookie(filterContext.HttpContext.Request);

            string culture = (cultureOnCookie == string.Empty)
            ? (filterContext.RouteData.Values["lang"].ToString())
            : cultureOnCookie;

            SetCurrentCultureOnThread(culture);

            base.OnActionExecuting(filterContext);
        }

        private static void SetCurrentCultureOnThread(string lang)
        {
            if (string.IsNullOrEmpty(lang))
                lang = GlobalHelper.DefaultCulture;
            var cultureInfo = new System.Globalization.CultureInfo(lang);
            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
            System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
        }

        public static String GetCultureOnCookie(HttpRequestBase request)
        {
            var cookie = request.Cookies[_cookieLangName];
            string culture = string.Empty;
            if (cookie != null)
            {
                culture = cookie.Value;
            }
            return culture;
        }
    }

    public class GlobalHelper
    {
        public static string CurrentCulture
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
        }

        public static string DefaultCulture
        {
            get
            {
                Configuration config = WebConfigurationManager.OpenWebConfiguration("/");
                GlobalizationSection section = (GlobalizationSection)config.GetSection("system.web/globalization");
                return section.UICulture;
            }
        }
    }
}
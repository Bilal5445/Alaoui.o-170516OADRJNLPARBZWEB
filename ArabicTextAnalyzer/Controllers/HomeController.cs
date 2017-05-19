using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArabicTextAnalyzer.Business.Provider;

namespace ArabicTextAnalyzer.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var textSentimentAnalyzer = new TextSentimentAnalyzer();
            var textEntityExtraction = new TextEntityExtraction();

            var a = textSentimentAnalyzer.GetSentiment("sdasd");
            var b = textEntityExtraction.GetEntities("sad");

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public JsonResult AjaxMethod(string name)
        {
            //PersonModel person = new PersonModel
            //{
            //    Name = name,
            //    DateTime = DateTime.Now.ToString()
            //};
            return Json(new {test = "test" });
        }
    }
}
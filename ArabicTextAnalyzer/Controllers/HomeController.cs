using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using ArabicTextAnalyzer.Business.Provider;
using ArabicTextAnalyzer.Models;

namespace ArabicTextAnalyzer.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var textConverter = new TextConverter();
  
            return View();
        }

        [HttpGet]
        public ActionResult ProcessText(string text)
        {
            var textSentimentAnalyzer = new TextSentimentAnalyzer();
            var textEntityExtraction = new TextEntityExtraction();

            var sentiment = textSentimentAnalyzer.GetSentiment(text);
            var entities = textEntityExtraction.GetEntities(text);

            var result = new TextAnalyze
            {
                Entities = entities.ToList(),
                Sentiment = sentiment
            };

            return Json(result, JsonRequestBehavior.AllowGet);
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
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using ArabicTextAnalyzer.Business.Provider;
using ArabicTextAnalyzer.Models;

namespace ArabicTextAnalyzer.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // return View();
            // return RedirectToAction("Index", "Train");
            return RedirectToAction("IndexTranslateArabizi", "Train");
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult ProcessText([FromBody] string text)
        {
            // Arabizi to arabic script
            // either direct call to perl script
            var textConverter = new TextConverter();
            // or via a php api
            // var textConverter = new ApiTextConverter();

            // SA & Entity
            var textSentimentAnalyzer = new TextSentimentAnalyzer();
            var textEntityExtraction = new TextEntityExtraction();
            // Arabizi to arabic from perl script
            var arabicText = textConverter.Convert(text);

            // Sentiment analysis from watson https://gateway.watsonplatform.net/";
            var sentiment = textSentimentAnalyzer.GetSentiment(arabicText);

            // Entity extraction from rosette (https://api.rosette.com/rest/v1/)
            var entities = textEntityExtraction.GetEntities(arabicText);

            var result = new TextAnalyze
            {
                ArabicText = arabicText,
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
    }
}
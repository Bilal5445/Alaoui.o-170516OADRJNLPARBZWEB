using System.Linq;
using System.Web.Helpers;
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
              return View();
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult ProcessText([FromBody] string text)
        {

            var textConverter = new TextConverter();
            var textSentimentAnalyzer = new TextSentimentAnalyzer();
            var textEntityExtraction = new TextEntityExtraction();

            var arabicText = textConverter.Convert(text);

            var sentiment = textSentimentAnalyzer.GetSentiment(arabicText);
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
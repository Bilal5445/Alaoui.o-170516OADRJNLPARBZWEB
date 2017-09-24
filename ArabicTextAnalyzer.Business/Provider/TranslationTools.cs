using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TranslationTools
    {
        String spellCheckAPi = "https://api.cognitive.microsoft.com/bing/v5.0/spellcheck?text=";
        String bingSpellApiKey = ConfigurationManager.AppSettings["BingSpellcheckAPIKey"].ToString();

        public String CorrectTranslate(String arabiziWord, HttpServerUtilityBase Server)
        {
            // See if we can further correct/translate any latin words
            var previousText = arabiziWord;
            string spellcheckUrl = spellCheckAPi + previousText
                + "&cc=FR"
                + "&mkt=fr-FR"
                + "&mode=spell";
            var correctedWord = new BingSpellCheckerApiTools().bingSpellcheckApi(spellcheckUrl, bingSpellApiKey);
            if (correctedWord != previousText)
            {
                if (correctedWord == "")
                    correctedWord = previousText;
            }

            var translatedLatinWord = new GoogleTranslationApiTools().getArabicTranslatedWord(correctedWord, Server);
            // ArabicDarijaText = ArabicDarijaText.Replace(previousText, TranslatedText);

            return translatedLatinWord;
        }
    }

    public class GoogleTranslationApiTools
    {
        public string getArabicTranslatedWord(string correctedWord, HttpServerUtilityBase Server)
        {
            string translationApiKey = ConfigurationManager.AppSettings["GoogleTranslationApiKey"].ToString();
            string translateApiUrl = "https://translation.googleapis.com/language/translate/v2?key=" + translationApiKey;
            translateApiUrl += "&target=" + "AR";
            translateApiUrl += "&source=" + "FR";
            translateApiUrl += "&q=" + Server.UrlEncode(correctedWord.Trim());
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            string Jsonresult = client.DownloadString(translateApiUrl);
            JsonData jsonData = new JsonData();
            jsonData = (new JavaScriptSerializer()).Deserialize<JsonData>(Jsonresult);
            var TranslatedText = jsonData.Data.Translations[0].TranslatedText;
            translateApiUrl = "";
            return TranslatedText;
        }
    }

    public class BingSpellCheckerApiTools
    {
        public string bingSpellcheckApi(string url, string apiKey)
        {
            var correctedText = "";
            WebClient client = new WebClient();
            client.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
            client.Encoding = System.Text.Encoding.UTF8;
            try
            {
                string json = client.DownloadString(url);
                var jsonresult = JObject.Parse(json).SelectToken("flaggedTokens") as JArray;
                foreach (var result in jsonresult)
                {
                    correctedText = result.SelectToken("suggestions[0].suggestion").ToString();
                    return correctedText;
                }
                return correctedText;
            }
            catch (Exception ex)
            {
                throw ex;
                // return ex.ToString();
            }
        }
    }

    #region BACK YARD BO
    public class JsonData
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public List<Translation> Translations { get; set; }
    }

    public class Translation
    {
        public string TranslatedText { get; set; }
    }
    #endregion
}

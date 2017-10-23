﻿using Newtonsoft.Json.Linq;
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
        // String spellCheckAPi = "https://api.cognitive.microsoft.com/bing/v5.0/spellcheck?text=";
        String bingSpellApiKey; // = ConfigurationManager.AppSettings["BingSpellcheckAPIKey"].ToString();

        // google translation api key
        String translationApiKey; // = ConfigurationManager.AppSettings["GoogleTranslationApiKey"].ToString();

        // for UT only
        public TranslationTools(String bingSpellApiKey, String googleTranslationApiKey)
        {
            this.bingSpellApiKey = bingSpellApiKey;
            this.translationApiKey = googleTranslationApiKey;
        }

        public TranslationTools()
        {
            bingSpellApiKey = ConfigurationManager.AppSettings["BingSpellcheckAPIKey"].ToString();
            translationApiKey = ConfigurationManager.AppSettings["GoogleTranslationApiKey"].ToString();
        }

        public String CorrectTranslate(String arabiziWord)
        {
            // See if we can further correct/translate any latin words
            /*var previousText = arabiziWord;
            string spellcheckUrl = spellCheckAPi + WebUtility.UrlEncode(previousText)
                + "&cc=FR"
                + "&mkt=fr-FR"
                + "&mode=spell";*/
            var correctedWord = new BingSpellCheckerApiTools().bingSpellcheckApi(/*spellcheckUrl,*/arabiziWord, bingSpellApiKey);
            /*if (correctedWord != previousText)
            {
                if (correctedWord == "")
                    correctedWord = previousText;
            }*/

            var translatedLatinWord = new GoogleTranslationApiTools(translationApiKey).getArabicTranslatedWord(correctedWord);

            return translatedLatinWord;
        }
    }

    public class GoogleTranslationApiTools
    {
        string translationApiKey;

        public GoogleTranslationApiTools(String googleTranslationApiKey)
        {
            translationApiKey = googleTranslationApiKey;
        }

        public string getArabicTranslatedWord(string correctedWord)
        {
            string translateApiUrl = "https://translation.googleapis.com/language/translate/v2?key=" + translationApiKey;
            translateApiUrl += "&target=" + "AR";
            translateApiUrl += "&source=" + "FR";
            // translateApiUrl += "&model=" + "base";  // base = statistique, nmt = NN : MC181017 as of today, for target = arabic, only base is available
            translateApiUrl += "&q=" + WebUtility.UrlEncode(correctedWord.Trim());
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
        public string bingSpellcheckApi(/*string url, */String arabiziWord, string apiKey)
        {
            String spellCheckAPi = "https://api.cognitive.microsoft.com/bing/v5.0/spellcheck?text=";

            var previousText = arabiziWord;
            string spellcheckUrl = spellCheckAPi + WebUtility.UrlEncode(previousText)
                + "&cc=FR"
                + "&mkt=fr-FR"
                + "&mode=spell";

            String url = spellcheckUrl;

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
                    break;
                    // return correctedText;
                }

                //
                if (correctedText != previousText)
                {
                    if (correctedText == "")
                        correctedText = previousText;
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

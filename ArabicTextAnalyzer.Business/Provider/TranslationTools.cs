using Newtonsoft.Json.Linq;
using OADRJNLPCommon.Business;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TranslationTools
    {
        // bing spellcheck api key
        String bingSpellApiKey;

        // google translation api key
        String translationApiKey;

        // for log
        HttpServerUtilityBase _server = null;

        // for UT only
        public TranslationTools(String bingSpellApiKey, String googleTranslationApiKey)
        {
            this.bingSpellApiKey = bingSpellApiKey;
            this.translationApiKey = googleTranslationApiKey;
        }

        public TranslationTools(HttpServerUtilityBase server)
        {
            bingSpellApiKey = ConfigurationManager.AppSettings["BingSpellcheckAPIKey"].ToString();
            translationApiKey = ConfigurationManager.AppSettings["GoogleTranslationApiKey"].ToString();

            _server = server;
        }

        public String CorrectTranslate(String arabiziWord)
        {
            // See if we can further correct/translate any latin words

            // bing spell
            var correctedWord = new BingSpellCheckerApiTools(_server).bingSpellcheckApi(arabiziWord, bingSpellApiKey);

            // google tranlsation
            var translatedLatinWord = new GoogleTranslationApiTools(translationApiKey).getArabicTranslatedWord(correctedWord, "html");

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

        public string getArabicTranslatedWord(string correctedWord, string format = null)
        {
            // see 1079 : codeswitch>google-translate : disable google-translate api from google because of cost passive consumption and trial expiration
            return correctedWord;

            string translateApiUrl = "https://translation.googleapis.com/language/translate/v2?key=" + translationApiKey;
            translateApiUrl += "&target=" + "AR";
            translateApiUrl += "&source=" + "FR";
            // translateApiUrl += "&model=" + "base";  // base = statistique, nmt = NN : MC181017 as of today, for target = arabic, only base is available
            translateApiUrl += "&q=" + WebUtility.UrlEncode(correctedWord.Trim(new char[] { ' ', '\t' }));

            // plain text or html format (for ignoring part)
            if (!String.IsNullOrEmpty(format) && (format == "html" || format == "text"))
                translateApiUrl += "&format=" + format;

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
        // for log
        HttpServerUtilityBase _server = null;

        public BingSpellCheckerApiTools()
        {
        }

        public BingSpellCheckerApiTools(HttpServerUtilityBase server)
        {
            _server = server;
        }

        public string bingSpellcheckApi(String arabiziWord, string apiKey)
        {
            // see 1078 : codeswitch>bing : disable Bing Checkspell api from azure because of cost passive consumption
            return arabiziWord;

            String spellCheckAPi = "https://api.cognitive.microsoft.com/bing/v5.0/spellcheck?text=";

            // clean
            arabiziWord = arabiziWord.Trim(new char[] { ' ', '\t' });

            var previousText = arabiziWord;
            string spellcheckUrl = spellCheckAPi + WebUtility.UrlEncode(previousText)
                + "&cc=FR"
                + "&mkt=fr-FR"
                + "&mode=spell";

            String url = spellcheckUrl;

            var correctedText = String.Empty;
            var supportText = String.Empty;
            WebClient client = new WebClient();
            client.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
            client.Encoding = System.Text.Encoding.UTF8;

            try
            {
                string json = client.DownloadString(url);
                var jsonresult = JObject.Parse(json).SelectToken("flaggedTokens") as JArray;

                // process : build a string with pieces from suggesestions from bing
                var peekindex = 0;
                foreach (var result in jsonresult)
                {
                    // index of first word with possible spelling correction
                    var stopindex = Convert.ToInt32(result.SelectToken("offset").ToString());

                    // copy string up to this location
                    correctedText += arabiziWord.Substring(peekindex, (stopindex - peekindex));
                    supportText += arabiziWord.Substring(peekindex, (stopindex - peekindex));

                    //
                    var token = result.SelectToken("token").ToString();
                    var firstsuggestion = result.SelectToken("suggestions[0].suggestion").ToString();
                    correctedText += firstsuggestion;
                    supportText += token;
                    peekindex = supportText.Length;
                }

                // copy the rest
                correctedText += arabiziWord.Substring(peekindex, (arabiziWord.Length - peekindex));

                //
                if (previousText != String.Empty && correctedText == String.Empty)
                    correctedText = previousText;

                return correctedText;
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse)wex.Response)
                    {
                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            var jsonerror = reader.ReadToEnd();
                            if (_server != null)
                            {
                                Logging.Write(_server, jsonerror);
                                Logging.Write(_server, wex.GetType().Name);
                                Logging.Write(_server, wex.Message);
                                Logging.Write(_server, wex.StackTrace);
                            }
                        }
                    }
                }
                else
                    throw;

                return previousText;
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

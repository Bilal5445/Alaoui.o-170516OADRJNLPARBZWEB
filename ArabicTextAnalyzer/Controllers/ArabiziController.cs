using ArabicTextAnalyzer.Business.Provider;
using ArabicTextAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Http;
using System.Configuration;
using System.Collections.Specialized;
using ArabicTextAnalyzer.BO;
using System.Web.Http.Description;
using System.Data.SqlClient;
using Dapper;
using OADRJNLPCommon.Models;
using OADRJNLPCommon.Business;
using System.Web;
using Newtonsoft.Json;

namespace ArabicTextAnalyzer.Controllers
{
    public class ArabiziController : ApiController
    {
        private static Object thisLock = new Object();

        /// <summary>
        /// Translate Arabizi text to Arabic text.
        /// </summary>
        /// <param name="token">The authentication token to use the Arabizi api.</param>
        /// <param name="text">The text to be translated to Arabic text.</param>
        [HttpGet]
        public IHttpActionResult Translate(/*[FromBody]*/string token, String text)
        {
            // are we allowed to use the API
            var errorMessage = string.Empty;
            if (!ValidateToken(token, "Translate", out errorMessage))
            {
                dynamic errexpando = new ExpandoObject();
                errexpando.StatusCode = HttpStatusCode.NotAcceptable;
                errexpando.ErrorMessage = errorMessage.Trim(new char[] { '\"' });
                return Ok(errexpando);
            }

            //
            M_ARABIZIENTRY arabiziEntry = new M_ARABIZIENTRY
            {
                ArabiziText = text,
                ArabiziEntryDate = DateTime.Now
            };

            // call real work
            // use expando to merge the json ouptuts : arabizi + arabic + latin words
            dynamic expando = new Arabizer().train(arabiziEntry, null, thisLock: thisLock);

            // keep only arabizi + arabic + latin
            expando.M_ARABICDARIJAENTRY_TEXTENTITYs = null;

            //
            return Ok(expando);
        }

        /// <summary>
        /// Extract NERs from text.
        /// </summary>
        /// <param name="token">The authentication token to use the Arabizi api.</param>
        /// <param name="text">The text from which NERs are extracted.</param>
        [HttpGet]
        public IHttpActionResult ExtractNers(/*[FromBody]*/string token, String text)
        {
            // are we allowed to use the API
            var errorMessage = string.Empty;
            if (!ValidateToken(token, "Extract", out errorMessage))
            {
                dynamic errexpando = new ExpandoObject();
                errexpando.StatusCode = HttpStatusCode.NotAcceptable;
                errexpando.ErrorMessage = errorMessage.Trim(new char[] { '\"' });
                return Ok(errexpando);
            }

            //
            M_ARABIZIENTRY arabiziEntry = new M_ARABIZIENTRY
            {
                ArabiziText = text,
                ArabiziEntryDate = DateTime.Now
            };

            // call real work
            // use expando to merge the json ouptuts : arabizi + arabic + latin words
            // plus also M_ARABICDARIJAENTRY_TEXTENTITYs
            dynamic expando = new Arabizer().train(arabiziEntry, null, thisLock: thisLock);

            // keep only arabizi + arabic + ner
            expando.M_ARABICDARIJAENTRY_LATINWORDs = null;

            //
            return Ok(expando);
        }

        /// <summary>
        /// Extract positive/negative NERs from text.
        /// </summary>
        /// <param name="token">The authentication token to use the Arabizi api.</param>
        /// <param name="text">The text from which positive/negative NERs are extracted.</param>
        [HttpGet]
        public IHttpActionResult ExtractSaNers(/*[FromBody]*/string token, String text)
        {
            // are we allowed to use the API
            var errorMessage = string.Empty;
            if (!ValidateToken(token, "Extract", out errorMessage))
            {
                dynamic errexpando = new ExpandoObject();
                errexpando.StatusCode = HttpStatusCode.NotAcceptable;
                errexpando.ErrorMessage = errorMessage.Trim(new char[] { '\"' });
                return Ok(errexpando);
            }

            //
            M_ARABIZIENTRY arabiziEntry = new M_ARABIZIENTRY
            {
                ArabiziText = text,
                ArabiziEntryDate = DateTime.Now
            };

            // call real work
            // use expando to merge the json ouptuts : arabizi + arabic + latin words
            // plus also M_ARABICDARIJAENTRY_TEXTENTITYs
            dynamic expando = new Arabizer().train(arabiziEntry, null, thisLock: thisLock);

            // keep only arabizi + arabic + ner
            expando.M_ARABICDARIJAENTRY_LATINWORDs = null;

            // limit to positive/negative ner
            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = expando.M_ARABICDARIJAENTRY_TEXTENTITYs;
            textEntities.RemoveAll(m => m.TextEntity.Type != "NEGATIVE" && m.TextEntity.Type != "POSITIVE" && m.TextEntity.Type != "SUPPORT" && m.TextEntity.Type != "SENSITIVE" && m.TextEntity.Type != "OPPOSE" && m.TextEntity.Type != "EXPLETIVE");
            expando.M_ARABICDARIJAENTRY_TEXTENTITYs = textEntities;

            //
            return Ok(expando);
        }

        /// <summary>
        /// Obtain the authentication token to use the Arabizi api.
        /// </summary>
        /// <param name="clientId">The client id : available in the dashboard</param>
        /// <param name="clientSecret">The client secret : available in the dashboard</param>
        // POST: api/Authenticate
        [HttpGet]
        public IHttpActionResult Authenticate(string clientId, string clientSecret)
        {
            var response = CheckForToken(clientId, clientSecret);

            return Ok(response);
        }

        /*[HttpGet]
        public IHttpActionResult GetArabicDarijaEntryForFbPost(String text)
        {
            var errorMessage = string.Empty;
            //if (ValidateToken(token, "GetArabicDarijaEntry", out errorMessage))
            //{
            M_ARABICDARIJAENTRY arabicDarijaEntry = null;

            M_ARABIZIENTRY arabiziEntry = new M_ARABIZIENTRY
            {
                ArabiziText = text,
                ArabiziEntryDate = DateTime.Now
            };

            // Arabizi to arabic script via direct call to perl script
            var textConverter = new TextConverter();

            //
            List<M_ARABICDARIJAENTRY_LATINWORD> arabicDarijaEntryLatinWords = new List<M_ARABICDARIJAENTRY_LATINWORD>();

            // Arabizi to arabic from perl script
            if (arabiziEntry.ArabiziText != null)
            {
                lock (thisLock)
                {
                    // complete arabizi entry
                    arabiziEntry.ID_ARABIZIENTRY = Guid.NewGuid();

                    // prepare darija from perl script
                    var arabicText = textConverter.Convert(arabiziEntry.ArabiziText);
                    arabicDarijaEntry = new M_ARABICDARIJAENTRY
                    {
                        ID_ARABICDARIJAENTRY = Guid.NewGuid(),
                        ID_ARABIZIENTRY = arabiziEntry.ID_ARABIZIENTRY,
                        ArabicDarijaText = arabicText
                    };

                    // Save arabiziEntry to Serialization
                    String path = HostingEnvironment.MapPath("~/App_Data/data_M_ARABIZIENTRY.txt");
                    new TextPersist().Serialize<M_ARABIZIENTRY>(arabiziEntry, path);

                    // Save arabicDarijaEntry to Serialization
                    path = HostingEnvironment.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY.txt");
                    new TextPersist().Serialize<M_ARABICDARIJAENTRY>(arabicDarijaEntry, path);

                    // latin words
                    MatchCollection matches = TextTools.ExtractLatinWords(arabicDarijaEntry.ArabicDarijaText);

                    // save every match
                    // also calculate on the fly the number of varaiants
                    foreach (Match match in matches)
                    {
                        // do not consider words in the bidict as latin words
                        if (new TextFrequency().BidictContainsWord(match.Value))
                            continue;

                        String arabiziWord = match.Value;
                        int variantsCount = new TextConverter().GetAllTranscriptions(arabiziWord).Count;

                        var latinWord = new M_ARABICDARIJAENTRY_LATINWORD
                        {
                            ID_ARABICDARIJAENTRY_LATINWORD = Guid.NewGuid(),
                            ID_ARABICDARIJAENTRY = arabicDarijaEntry.ID_ARABICDARIJAENTRY,
                            LatinWord = arabiziWord,
                            VariantsCount = variantsCount
                        };

                        //
                        arabicDarijaEntryLatinWords.Add(latinWord);

                        // Save to Serialization
                        path = HostingEnvironment.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY_LATINWORD.txt");
                        new TextPersist().Serialize<M_ARABICDARIJAENTRY_LATINWORD>(latinWord, path);
                    }
                }
            }

            //
            if (arabicDarijaEntry == null)
            {
                return NotFound();
            }
            // return Ok(arabicDarijaEntry);

            // use expando to merge the json ouptuts : arabizi + arabic + latin words
            dynamic expando = new ExpandoObject();
            expando.M_ARABIZIENTRY = arabiziEntry;
            expando.M_ARABICDARIJAENTRY = arabicDarijaEntry;
            expando.M_ARABICDARIJAENTRY_LATINWORD = arabicDarijaEntryLatinWords;
            return Ok(expando);
            //}
            //else
            //{
            //    var message = new HttpResponseMessage();
            //    message.StatusCode = HttpStatusCode.NotAcceptable;
            //    message.Content = new StringContent(errorMessage);
            //    return Ok(message);
            //}
        }*/

        [ApiExplorerSettings(IgnoreApi = true)]
        public bool ValidateToken(string token, string methodToCall, out string errorMessage)
        {
            errorMessage = string.Empty;
            string result = null;

            var requestContent = "/?token=" + token + "&methodTocall=" + methodToCall;
            var url = ConfigurationManager.AppSettings["AuththenticationDomain"] + "/" + "api/Authenticate/ValidateToken" + requestContent;
            result = HtmlHelpers.PostAPIRequest(url, requestContent);

            if (result.Contains("Success"))
                return true;
            else
            {
                errorMessage = result;
                return false;
            }
        }

        private string CheckForToken(string clientId, string clientSecret)
        {
            string result = null;

            var requestContent = "/?clientId=" + clientId + "&clientSecret=" + clientSecret;

            var url = ConfigurationManager.AppSettings["AuththenticationDomain"] + "/" + "api/Authenticate/Authenticate" + requestContent;
            result = HtmlHelpers.PostAPIRequest(url, requestContent);

            return result;
        }

        /// <summary>
        /// Obtain the data source for the NER Count Per Theme query
        /// </summary>
        /// <param name="ID_XTRCTTHEME">The client theme id</param>
        [HttpGet]
        public IHttpActionResult StatNerCountPerTheme(string ID_XTRCTTHEME)
        {
            try
            {
                return Ok(new Arabizer().StatNerCountPerTheme(ID_XTRCTTHEME));
            }
            catch (SqlException ex)
            {
                var Server = HttpContext.Current.Server;
                Logging.Write(Server, ex.GetType().Name);
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return InternalServerError(ex);
            }
            catch (Exception ex)
            {
                var Server = HttpContext.Current.Server;
                Logging.Write(Server, ex.GetType().Name);
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                /*return Ok(JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = ex.Message
                }));*/
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Obtain the data source for the NER Count For All Themes query
        /// </summary>
        [HttpGet]
        public IHttpActionResult StatNerCountPerThemes()
        {
            // TODO : needs authprization
            try
            {
                String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    String qry0 = "SELECT ID_XTRCTTHEME, Keyword, SUM(Keyword_Count) CountPerKeyword FROM T_XTRCTTHEME_KEYWORD "
                                + "GROUP BY ID_XTRCTTHEME, Keyword "
                                + "ORDER BY SUM(Keyword_Count) DESC ";

                    // DBG
                    var Server = HttpContext.Current.Server;
                    Logging.Write(Server, qry0);

                    conn.Open();
                    return Ok(conn.Query<LM_CountPerKeywordPerTheme>(qry0));
                }
            }
            catch (SqlException ex)
            {
                var Server = HttpContext.Current.Server;
                Logging.Write(Server, ex.GetType().Name);
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return InternalServerError(ex);
            }
            catch (Exception ex)
            {
                var Server = HttpContext.Current.Server;
                Logging.Write(Server, ex.GetType().Name);
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                /*return Ok(JsonConvert.SerializeObject(new
                {
                    status = false,
                    message = ex.Message
                }));*/
                return InternalServerError(ex);
            }
        }
    }
}

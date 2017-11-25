﻿using ArabicTextAnalyzer.Business.Provider;
using ArabicTextAnalyzer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Configuration;
using System.Collections.Specialized;

namespace ArabiziWebAPI.Controllers
{
    public class ArabiziController : ApiController
    {
        private static Object thisLock = new Object();

        M_ARABICDARIJAENTRY[] _arabicdarijaentries = new M_ARABICDARIJAENTRY[]
        {
            new M_ARABICDARIJAENTRY { ID_ARABIZIENTRY = Guid.NewGuid(), ID_ARABICDARIJAENTRY = Guid.NewGuid(), ArabicDarijaText = "السلام" },
            new M_ARABICDARIJAENTRY { ID_ARABIZIENTRY = Guid.NewGuid(), ID_ARABICDARIJAENTRY = Guid.NewGuid(), ArabicDarijaText = "سلام" }
        };

        public IEnumerable<M_ARABICDARIJAENTRY> GetAllArabicDarijaEntries()
        {
            return _arabicdarijaentries;
        }

        public IHttpActionResult Authenticate(string clientId, string clientSecret)
        {
            var response = CheckForToken(clientId, clientSecret);

            return Ok(response);
        }


        public IHttpActionResult GetArabicDarijaEntry(string token, int? id)
        {
            var errorMessage = string.Empty;
            if (ValidateToken(token, "GetArabicDarijaEntry", out errorMessage))
            {
                // var arabicdarijaentry = _arabicdarijaentries.FirstOrDefault((p) => p.Id == id);
                var arabicdarijaentry = _arabicdarijaentries[0];
                if (arabicdarijaentry == null)
                {
                    return NotFound();
                }
                return Ok(arabicdarijaentry);
            }
            else
            {
                HttpResponseMessage response = new HttpResponseMessage();
                response = Request.CreateResponse(HttpStatusCode.NotAcceptable, errorMessage);
                return Ok(new { Status = "Forbidden", Error = errorMessage });
            }
        }


        public IHttpActionResult GetArabicDarijaEntry([FromBody]string token, String text)
        {
            var errorMessage = string.Empty;
            if (ValidateToken(token, "GetArabicDarijaEntry", out errorMessage))
            {
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
                        /*var*/
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
            }
            else
            {
                var message = new HttpResponseMessage();
                message.StatusCode = HttpStatusCode.NotAcceptable;
                message.Content = new StringContent(errorMessage);
                return Ok(message);
            }
        }

        public bool ValidateToken(string token, string methodToCall, out string errorMessage)
        {
            errorMessage = string.Empty;
            string result = null;
            //Dictionary<string, string> requestContent = new Dictionary<string, string>();
            //requestContent.Add("token", token);
            //requestContent.Add("methodTocall", methodToCall);

            var requestContent = "/?token=" + token + "&methodTocall=" + methodToCall;
            result = HtmlHelpers.PostAPIRequest(ConfigurationManager.AppSettings["AuththenticationDomain"] + "/" + "api/Authenticate/ValidateToken"
               + requestContent, requestContent);

            if (result.Contains("Success"))
            {
                return true;
            }
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

            result = HtmlHelpers.PostAPIRequest(ConfigurationManager.AppSettings["AuththenticationDomain"] + "/" +
                "api/Authenticate/Authenticate" + requestContent, requestContent);

            return result;

        }
    }


    public static class HtmlHelpers
    {

        public static string PostAPIRequest(string url, string para)
        {
            HttpClient client;
            string result = string.Empty;
            try
            {
                client = new HttpClient();
                client.DefaultRequestHeaders.Clear();
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");


                byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(para);
                var content = new ByteArrayContent(messageBytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                content.Headers.Add("access-control-allow-origin", "*");


                var response = client.PostAsync(url, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    result = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result;

        }

        public static string MakeHttpClientRequest(string requestUrl, Dictionary<string, string> requestContent, HttpMethod verb)
        {
            string result = string.Empty;
            using (WebClient client1 = new WebClient())
            {
                try
                {

                    var requestData = new NameValueCollection();
                    if (requestContent != null)
                    {
                        foreach (var item in requestContent)
                        {
                            requestData.Add(item.Key, item.Value);
                        }
                    }
                    byte[] response1 = client1.UploadValues(requestUrl, requestData);

                    result = System.Text.Encoding.UTF8.GetString(response1);
                }
                catch (Exception ex)
                {


                }

            }
            return result;


        }
    }
}



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

        public IHttpActionResult GetArabicDarijaEntry(string token,int id)
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
                var message = new HttpResponseMessage();
                message.StatusCode = HttpStatusCode.NotAcceptable;
                message.Content = new StringContent(errorMessage);
                return Ok(message);
            }
        }

        public IHttpActionResult GetArabicDarijaEntry(string token, String text)
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

        public IHttpActionResult Authenticate(string clientId, string clientSecret)
        {
            var response = CheckForToken(clientId, clientSecret);
            return Ok(response);
        }
        public bool ValidateToken(string token, string methodToCall, out string errorMessage)
        {
            errorMessage = string.Empty;
            string result = null;
            Dictionary<string, string> requestContent = new Dictionary<string, string>();
            requestContent.Add("token", token);
            requestContent.Add("methodTocall", methodToCall);
            result = HtmlHelpers.MakeHttpClientRequest(ConfigurationManager.AppSettings["AuththenticationDomain"] + "/" + "api/ValidateToken",
               requestContent, HttpMethod.Post);

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
            Dictionary<string, string> requestContent = new Dictionary<string, string>();
            requestContent.Add("ClientId", clientId);
            requestContent.Add("ClientSecret", clientSecret);
            result = HtmlHelpers.MakeHttpClientRequest(ConfigurationManager.AppSettings["AuththenticationDomain"] + "/" + "api/Authenticate",
               requestContent, HttpMethod.Post);

            return result;

        }
    }


    public static class HtmlHelpers
    {

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



﻿using ArabicTextAnalyzer.Business.Provider;
using ArabicTextAnalyzer.Domain;
using ArabicTextAnalyzer.Domain.Models;
using ArabicTextAnalyzer.Models.Repository;
using ArabicTextAnalyzer.Models;
using ArabicTextAnalyzer.ViewModels;
using Dapper;
using Newtonsoft.Json;
using OADRJNLPCommon.Business;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using Microsoft.AspNet.Identity;
using OADRJNLPCommon.Models;
using ArabicTextAnalyzer.Business.ScrappyBusiness;
using System.Net.Http;
using System.Net;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using ArabicTextAnalyzer.Contracts;

namespace ArabicTextAnalyzer.Controllers
{
    [Authorize]
    public class TrainController : Controller
    {
        // global lock to queur concurrent access
        private static Object thisLock = new Object();

        // authentication in constructor
        IAuthenticate _IAuthenticate;
        IRegisterApp _IRegister;
        public TrainController()
        {
            _IAuthenticate = new AuthenticateConcrete();
            _IRegister = new RegisterAppConcrete();
        }

        // data access mode (ef-sql, dapper-sql, xml)
        // AccessMode _accessMode = AccessMode.dappersql;

        // GET: Train
        [Authorize]
        public ActionResult Index()
        {
            //
            var userId = User.Identity.GetUserId();

            // token management
            var token = Session["_T0k@n_"];
            bool istokenexpire = _IAuthenticate.IsTokenExpire(Convert.ToString(token));
            if (token == null || istokenexpire)
            {
                // MC191217 tokens are good to track who calls the arabizi function (api or web app), but web app user does not need to generate manually a token,
                // the fact that he is logged on, means he should be able to use translation, means a token should be always ready for him. Solution (workaround?) generate
                // on the fly a new token each time the token is expired

                /*Session["_T0k@n_"] = String.Empty;
                Session["message"] = String.Empty;
                TempData["showAlertWarning"] = true;
                TempData["msgAlert"] = "Your token is expired.";*/

                // generate token (and fill session token)
                var clientKeyToolkit = new ClientKeysConcrete();
                var clientkeys = clientKeyToolkit.GetGenerateUniqueKeyByUserID(userId);
                string message = string.Empty;
                bool isAppValid = clientKeyToolkit.IsAppValid(clientkeys);
                if (isAppValid == false)
                    message = "No More calls";
                else
                {
                    String sessiontoken;
                    String tokenExpiry = ConfigurationManager.AppSettings["TokenExpiry"];
                    var tokenmessage = new AppManager().GetToken(clientkeys, _IAuthenticate, tokenExpiry, out sessiontoken);
                    Session["_T0k@n_"] = sessiontoken;
                    if (String.IsNullOrEmpty(sessiontoken))
                        message = tokenmessage; // pass message only if token not generated
                }

                // fill session data
                Session["message"] = message;
                if (clientkeys != null)
                    Session["userId"] = clientkeys.UserID;
            }
            string errMessage = string.Empty;
            List<RegisterApp> _registerApp = _IRegister.ListofApps(userId).ToList();
            ViewBag.registerApp = _registerApp;

            // send size of corpus & co-data
            @ViewBag.CorpusSize = new TextFrequency().GetCorpusNumberOfLine();
            @ViewBag.CorpusWordCount = new TextFrequency().GetCorpusWordCount();
            @ViewBag.BidictSize = new TextFrequency().GetBidictNumberOfLine();
            var arabiziEntriesCount = loaddeserializeM_ARABIZIENTRY_DAPPERSQL().Count;
            @ViewBag.ArabiziEntriesCount = arabiziEntriesCount;
            @ViewBag.RatioLatinWordsOnEntries = loaddeserializeM_ARABICDARIJAENTRY_LATINWORD_DAPPERSQL().Where(m => String.IsNullOrWhiteSpace(m.Translation) == true).ToList().Count / (double)arabiziEntriesCount;

            @ViewBag.EntitiesCount = new TextFrequency().GetEntitiesCount();

            // deserialize/send twingly accounts
            @ViewBag.TwinglyAccounts = loaddeserializeM_TWINGLYACCOUNT_DAPPERSQL();

            // themes : deserialize/send list of themes, plus send active theme, plus send list of tags/keywords
            var xtrctThemes = loaddeserializeM_XTRCTTHEME_DAPPERSQL();
            // List<M_XTRCTTHEME_KEYWORD> xtrctThemesKeywords = loaddeserializeM_XTRCTTHEME_KEYWORD_DAPPERSQL();
            List<M_XTRCTTHEME_KEYWORD> xtrctThemesKeywords = loaddeserializeM_XTRCTTHEME_KEYWORD_Active_DAPPERSQL();
            var activeXtrctTheme = xtrctThemes.Find(m => m.CurrentActive == "active");
            @ViewBag.XtrctThemes = xtrctThemes;
            @ViewBag.XtrctThemesPlain = xtrctThemes.Select(m => new SelectListItem { Text = m.ThemeName.Trim(), Selected = m.ThemeName.Trim() == activeXtrctTheme.ThemeName.Trim() ? true : false });
            @ViewBag.ActiveXtrctTheme = activeXtrctTheme;
            // note the keywords can be many records associated with this theme, plus the original record (filled at creation) contains many kewords seprated by space
            // @ViewBag.ActiveXtrctThemeTags = String.Join(" ", xtrctThemesKeywords.Where(m => m.ID_XTRCTTHEME == activeXtrctTheme.ID_XTRCTTHEME).Select(m => m.Keyword).ToList()).Split(new char[] { ' ' }).ToList();
            // @ViewBag.ActiveXtrctThemeTags = String.Join(" ", xtrctThemesKeywords.Select(m => m.Keyword).ToList()).Split(new char[] { ' ' }).ToList();
            @ViewBag.ActiveXtrctThemeTags = xtrctThemesKeywords.Select(m => m.Keyword).ToList();

            // file upload communication
            @ViewBag.showAlertWarning = TempData["showAlertWarning"] != null ? TempData["showAlertWarning"] : false;
            @ViewBag.showAlertSuccess = TempData["showAlertSuccess"] != null ? TempData["showAlertSuccess"] : false;
            @ViewBag.msgAlert = TempData["msgAlert"] != null ? TempData["msgAlert"] : String.Empty;
            TempData.Remove("showAlertWarning");
            TempData.Remove("showAlertSuccess");
            TempData.Remove("msgAlert");

            // Fertch the data for fbPage as only for that theme
            if (token != null && !string.IsNullOrEmpty(token as string))
            {
                var fbFluencerAsTheme = loadAllT_Fb_InfluencerAsTheme();
                ViewBag.AllInfluence = fbFluencerAsTheme;
            }

            //
            return View();
        }

        [HttpPost]
        public ActionResult ArabicDarijaEntryPartialView()
        {
            try
            {
                var token = Session["_T0k@n_"];
                if (token != null && !string.IsNullOrEmpty(token as string))
                {
                    var fbFluencerAsTheme = loadAllT_Fb_InfluencerAsTheme();
                    ViewBag.AllInfluence = fbFluencerAsTheme;
                }
                // pass entries to partial view via the model (instead of the bag for a view)
                return PartialView("_IndexPartialPage_arabicDarijaEntries");
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return null;
            }
        }

        #region FRONT YARD ACTIONS TRAIN
        [Authorize]
        [HttpPost]
        public ActionResult TrainStepOne(M_ARABIZIENTRY arabiziEntry, String mainEntity)
        {
            //
            var userId = User.Identity.GetUserId();

            // token management
            var token = Session["_T0k@n_"];
            bool istokenexpire = _IAuthenticate.IsTokenExpire(Convert.ToString(token));
            if (token == null || istokenexpire)
            {
                // MC191217 tokens are good to track who calls the arabizi function (api or web app), but web app user does not need to generate manually a token,
                // the fact that he is logged on, means he should be able to use translation, means a token should be always ready for him. Solution (workaround?) generate
                // on the fly a new token each time the token is expired

                // generate token (and fill session token)
                var clientKeyToolkit = new ClientKeysConcrete();
                var clientkeys = clientKeyToolkit.GetGenerateUniqueKeyByUserID(userId);
                string message = string.Empty;
                bool isAppValid = clientKeyToolkit.IsAppValid(clientkeys);
                if (isAppValid == false)
                    message = "No More calls";
                else
                {
                    String sessiontoken;
                    String tokenExpiry = ConfigurationManager.AppSettings["TokenExpiry"];
                    var tokenmessage = new AppManager().GetToken(clientkeys, _IAuthenticate, tokenExpiry, out sessiontoken);
                    Session["_T0k@n_"] = sessiontoken;
                    token = sessiontoken;
                    if (String.IsNullOrEmpty(sessiontoken))
                        message = tokenmessage; // pass message only if token not generated
                }
            }

            // 
            string errMessage = string.Empty;
            if (token != null && _IAuthenticate.IsTokenValid(Convert.ToString(token), "TrainStepOne", out errMessage))
            {
                // Arabizi to arabic script via direct call to perl script
                var res = train(arabiziEntry, mainEntity);

                if (res == Guid.Empty)
                {
                    TempData["showAlertWarning"] = true;
                    TempData["msgAlert"] = "Text is required.";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                // Session["_T0k@n_"] = String.Empty;
                // Session["message"] = String.Empty;
                TempData["showAlertWarning"] = true;
                TempData["msgAlert"] = errMessage;  // "Not a valid token";
            }

            //
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Train_AddToCorpus(String arabiziWord, Guid arabiziWordGuid)
        {
            var textConverter = new TextConverter();
            String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";
            String twinglyApiUrl = "https://data.twingly.net/socialfeed/a/api/";

            // obtain current twingly key
            List<M_TWINGLYACCOUNT> twinglyAccounts = loaddeserializeM_TWINGLYACCOUNT_DAPPERSQL();
            String twinglyApiKey = twinglyAccounts.Find(m => m.CurrentActive == "active").ID_TWINGLYACCOUNT_API_KEY.ToString();

            // deserialize M_ARABICDARIJAENTRY_LATINWORD to check if most popular already found
            List<M_ARABICDARIJAENTRY_LATINWORD> latinWordsEntries = loaddeserializeM_ARABICDARIJAENTRY_LATINWORD_DAPPERSQL();
            var latinWordsEntry = latinWordsEntries.Find(m => m.ID_ARABICDARIJAENTRY_LATINWORD == arabiziWordGuid);
            if (latinWordsEntry == null)
            {
                ViewBag.MostPopularVariant = "No M_ARABICDARIJAENTRY_LATINWORD found";

                return RedirectToAction("Index");
            }
            if (String.IsNullOrEmpty(latinWordsEntry.MostPopularVariant) == false)
            {
                // ViewBag.MostPopularVariant = "MostPopularVariant already found";
                // return View("Index");
                // we already have the most popular
                // if not in corpus let us add it via twningly
                if (new TextFrequency().CorpusContainsWord(latinWordsEntry.MostPopularVariant))
                {
                    ViewBag.MostPopularVariant = "MostPopularVariant already found attached to latin word, and already in corpus. Try to convert again";

                    return RedirectToAction("Index");
                }
                else
                {
                    // lets look for a post via twigly to add to corpus
                    // 7 get a post containing this keyword
                    var postText = OADRJNLPCommon.Business.Business.getPostBasedOnKeywordFromFBViaTwingly(latinWordsEntry.MostPopularVariant, twinglyApi15Url, twinglyApiKey, true);
                    if (postText == String.Empty) // if no results, look everywhere
                        postText = OADRJNLPCommon.Business.Business.getPostBasedOnKeywordFromFBViaTwingly(latinWordsEntry.MostPopularVariant, twinglyApi15Url, twinglyApiKey, false);
                    // upddate count twingly account
                    new TwinglyTools().upddateCountTwinglyAccount(twinglyApiUrl, twinglyApiKey, Server.MapPath("~/App_Data/data_M_TWINGLYACCOUNT.txt"));
                    //
                    if (String.IsNullOrEmpty(postText))
                    {
                        ViewBag.MostPopularVariant = "No post found containing the most popular variant";

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // 8 add this post to dict
                        var textFrequency = new TextFrequency();
                        if (textFrequency.CorpusContainsSentence(postText) == false)
                            textFrequency.AddPhraseToCorpus(postText);

                        // 9 recompile the dict
                        textConverter.CatCorpusDict();  // 5s
                        textConverter.SrilmLmDict();    // 1mn25s => 55s

                        ViewBag.MostPopularVariant = "New post added into corpus with the most popular variant. Try to convert again";
                        ViewBag.Post = postText;

                        return RedirectToAction("Index");
                    }
                }
            }

            // standard behaviour when we are about to find the right variant
            var variants = textConverter.GetAllTranscriptions(arabiziWord);
            if (variants.Count > 100)
            {
                ViewBag.MostPopularVariant = "More than 100 variants";

                return RedirectToAction("Index");
            }
            else
            {
                // 5 get most popular keyword
                var mostPopularKeyword = OADRJNLPCommon.Business.Business.getMostPopularVariantFromFBViaTwingly(variants, twinglyApi15Url, twinglyApiKey);

                // upddate count twingly account
                new TwinglyTools().upddateCountTwinglyAccount(twinglyApiUrl, twinglyApiKey, Server.MapPath("~/App_Data/data_M_TWINGLYACCOUNT.txt"));

                //
                if (String.IsNullOrEmpty(mostPopularKeyword))
                {
                    ViewBag.MostPopularVariant = "No Most Popular Keyword";

                    return RedirectToAction("Index");
                }
                else
                {
                    if (arabiziWordGuid != Guid.Empty)
                    {
                        // save mostPopularKeyword
                        var path = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY_LATINWORD.txt");
                        new TextPersist().Serialize_Update_M_ARABICDARIJAENTRY_LATINWORD_MostPopularVariant(arabiziWordGuid, mostPopularKeyword, path);
                    }

                    //
                    ViewBag.MostPopularVariant = mostPopularKeyword;

                    return RedirectToAction("Index");
                }
            }
        }

        [HttpGet]
        public ActionResult Train_DeleteEntry(Guid arabiziWordGuid)
        {
            Serialize_Delete_M_ARABIZIENTRY_Cascading_EFSQL(arabiziWordGuid);

            //
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Train_DeleteEntries(String arabiziWordGuids)
        {
            var larabiziWordGuids = arabiziWordGuids.Split(new char[] { ',' });

            // var dataPath = Server.MapPath("~/App_Data/");

            foreach (var larabiziWordGuid in larabiziWordGuids)
            {
                // minor : trim leading '='
                Guid arabiziWordGuid = new Guid(larabiziWordGuid.TrimStart(new char[] { '=' }));

                // new TextPersist().Serialize_Delete_M_ARABIZIENTRY_Cascading(arabiziWordGuid, dataPath);
                Serialize_Delete_M_ARABIZIENTRY_Cascading_EFSQL(arabiziWordGuid);
            }

            //
            return RedirectToAction("Index");
        }

        // This action applies a new main tag/entity/theme/keyword to a post
        [HttpGet]
        public ActionResult Train_ApplyNewMainTag(Guid idArabicDarijaEntry, String mainEntity)
        {
            // Arg mainEntity is the active theme name
            var activeTheme = mainEntity;

            // MC111217 since we may have in the DB cases where there are more than one main entities for the same post (due to previous bug in code), we need to 
            // find them and to clean them (delete the extra main entities)
            var arabicDarijaEntryOtherMainEntities = loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY_DAPPERSQL().FindAll(m => m.ID_ARABICDARIJAENTRY == idArabicDarijaEntry && m.TextEntity.Mention != activeTheme && m.TextEntity.Type == "MAIN ENTITY");
            serialize_Delete_M_ARABICDARIJAENTRY_TEXTENTITY_EFSQL(arabicDarijaEntryOtherMainEntities);

            // load M_ARABICDARIJAENTRY_TEXTENTITY
            List<M_ARABICDARIJAENTRY_TEXTENTITY> arabicDarijaEntryTextEntities = loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY_DAPPERSQL();

            // Check before if this entry has already a main entity
            if (arabicDarijaEntryTextEntities.Find(m => m.ID_ARABICDARIJAENTRY == idArabicDarijaEntry && m.TextEntity.Mention == mainEntity && m.TextEntity.Type == "MAIN ENTITY") != null)
            {
                TempData["showAlertWarning"] = true;
                TempData["msgAlert"] = "'" + mainEntity + "' is already MAIN ENTITY for the post";
                return RedirectToAction("Index");
            }

            // apply new main tag
            var m_arabicdarijaentry_textentity = new M_ARABICDARIJAENTRY_TEXTENTITY
            {
                ID_ARABICDARIJAENTRY_TEXTENTITY = Guid.NewGuid(),
                ID_ARABICDARIJAENTRY = idArabicDarijaEntry,
                TextEntity = new TextEntity
                {
                    Count = 1,
                    Mention = mainEntity,
                    Type = "MAIN ENTITY"
                }
            };
            saveserializeM_ARABICDARIJAENTRY_TEXTENTITY_EFSQL(m_arabicdarijaentry_textentity);

            //
            TempData["showAlertSuccess"] = true;
            TempData["msgAlert"] = "'" + mainEntity + "' is now MAIN ENTITY for the post";
            return RedirectToAction("Index");
        }
        #endregion

        #region FRONT YARD ACTIONS FB
        // Method for translate the fb posts
        [HttpGet]
        public async Task<object> TranslateFbPost(String content, string id)
        {
            string errMessage = string.Empty;
            bool status = false;
            string translatedstring = "";

            var url = ConfigurationManager.AppSettings["TranslateDomain"] + "/" + "api/Arabizi/GetArabicDarijaEntryForFbPost?text=" + content;
            var result = await HtmlHelpers.PostAPIRequest(url, "", type: "GET");

            if (result.Contains("Success"))
            {
                status = true;
                translatedstring = result.Replace("Success", "");
                var @singleQuote = "CHAR(39)";
                translatedstring = translatedstring.Replace("'", "");
                var returndata = SaveTranslatedPost(id, translatedstring);
                if (returndata > 0)
                {
                    //
                }
                // return true;

                // MC081217 furthermore translate via train to populate NER, analysis data, ... (TODO LATER : should be the real code instead of API or API should do the real complete work)
                // Arabizi to arabic script via direct call to perl script
                var xtrctThemes = loaddeserializeM_XTRCTTHEME_DAPPERSQL();
                var activeXtrctTheme = xtrctThemes.Find(m => m.CurrentActive == "active");
                train(new M_ARABIZIENTRY
                {
                    ArabiziText = content.Trim(),
                    ArabiziEntryDate = DateTime.Now
                }, activeXtrctTheme.ThemeName);
            }
            else
            {
                errMessage = result;
                //return false;
            }

            return JsonConvert.SerializeObject(new
            {
                status = status,
                recordsFiltered = translatedstring,
                message = errMessage
            });

        }

        [HttpGet]
        public async Task<object> RetrieveFBPost(string influencerurl_name)
        {
            T_FB_INFLUENCER influencer = new T_FB_INFLUENCER();
            influencer.id = "";
            influencer.url_name = influencerurl_name;
            string errMessage = string.Empty;
            bool status = false;
            string translatedstring = "";

            string result = null;

            var url = ConfigurationManager.AppSettings["FBWorkingAPI"] + "/" + "Data/FetchFBInfluencerPosts?CallFrom=" + influencerurl_name;
            result = await HtmlHelpers.PostAPIRequest(url, "", type: "POST");

            if (result.ToLower().Contains("true"))
            {
                status = true;
                translatedstring = result;
                // return true;
            }
            else
            {
                errMessage = result;
                //return false;
            }

            return JsonConvert.SerializeObject(new
            {
                status = status,
                recordsFiltered = translatedstring,
                message = errMessage
            });
        }

        [HttpGet]
        public async Task<object> AddFBInfluencer(String url_name, String pro_or_anti)
        {
            String errMessage = string.Empty;
            bool status = false;
            String translatedstring = String.Empty;
            String result = null;

            //
            M_XTRCTTHEME activeTheme = loadDeserializeM_XTRCTTHEME_Active_DAPPERSQL();
            activeTheme = (activeTheme != null) ? activeTheme : new M_XTRCTTHEME();

            //
            Tuple<String, String> results = new Tuple<string, string>(String.Empty, String.Empty);

            //
            if (activeTheme.ID_XTRCTTHEME != null)
            {
                Logging.Write(Server, "AddFBInfluencer - 1");
                var themeid = activeTheme.ID_XTRCTTHEME;
                var url = ConfigurationManager.AppSettings["FBWorkingAPI"] + "/" + "AccountPanel/AddFBInfluencer?url_name=" + url_name + "&pro_or_anti=" + pro_or_anti + "&id=1&themeid=" + themeid + "&CallFrom=AddFBInfluencer";
                Logging.Write(Server, "AddFBInfluencer - 1.0.1 : " + url);
                results = await HtmlHelpers.PostAPIRequest_message(url, String.Empty, type: "POST");
                result = results.Item1;
                Logging.Write(Server, "AddFBInfluencer - 1.1");
            }
            else
            {
                Logging.Write(Server, "AddFBInfluencer - 2");
                result = "No theme id can get.";
                Logging.Write(Server, "AddFBInfluencer - 2.1");
            }

            if (result.ToLower().Contains("true"))
            {
                Logging.Write(Server, "AddFBInfluencer - 3");
                status = true;
                translatedstring = result;
                Logging.Write(Server, "AddFBInfluencer - 3.1");
            }
            else
            {
                Logging.Write(Server, "AddFBInfluencer - 4");
                status = false;
                errMessage = result;
                errMessage += " - " + results.Item2;
                Logging.Write(Server, "AddFBInfluencer - 4.0.1 : " + errMessage);
                Logging.Write(Server, "AddFBInfluencer - 4.1");
            }

            //
            Logging.Write(Server, "AddFBInfluencer - 5");
            return JsonConvert.SerializeObject(new
            {
                status = status,
                recordsFiltered = translatedstring,
                message = errMessage
            });
        }

        [HttpGet]
        public async Task<object> TranslateFbComments(String content, string ids)
        {
            string errMessage = string.Empty;
            bool status = false;
            string translatedstring = "";
            if (!string.IsNullOrEmpty(ids))
            {
                List<FBFeedComment> allComents = GetComments(ids);
                if (allComents != null && allComents.Count > 0)
                {
                    foreach (var item in allComents)
                    {
                        var url = ConfigurationManager.AppSettings["TranslateDomain"] + "/" + "api/Arabizi/GetArabicDarijaEntryForFbPost?text=" + item.message;
                        var result = await HtmlHelpers.PostAPIRequest(url, "", type: "GET");

                        if (result.Contains("Success"))
                        {
                            status = true;
                            translatedstring = result.Replace("Success", "");
                            var @singleQuote = "CHAR(39)";
                            translatedstring = translatedstring.Replace("'", "");
                            try
                            {
                                var returndata = SaveTranslatedComments(item.Id, translatedstring);
                                if (returndata > 0)
                                {
                                    //
                                }
                            }
                            catch (Exception e)
                            {
                                status = false;
                                errMessage = e.Message;
                            }

                            // return true;
                        }
                        else
                        {
                            errMessage = result;
                            //return false;
                        }
                    }
                }
                else
                {
                    errMessage = "All comments are already translated.";
                }


            }



            return JsonConvert.SerializeObject(new
            {
                status = status,
                recordsFiltered = translatedstring,
                message = errMessage
            });

        }
        #endregion

        #region FRONT YARD ACTIONS TWINGLY
        [HttpGet]
        public ActionResult TwinglySetup()
        {
            // use one time (from browser) to fill twingly accounts data - delete later - should have a UI to add new twinlgy accounts

            //
            var twinglyAccounts = new List<M_TWINGLYACCOUNT>();
            var contactATallpecmediaDOTcom = new M_TWINGLYACCOUNT
            {
                ID_TWINGLYACCOUNT_API_KEY = new Guid("E8E23C4E-F19E-4675-9FCB-D69B16ECB7AE"),
                UserName = "contact@allpecmedia.com",
                calls_free = 1000
            };

            //
            var alaouiDOToATgmailDOTcom = new M_TWINGLYACCOUNT
            {
                ID_TWINGLYACCOUNT_API_KEY = new Guid("0910880B-2D6A-47C2-8E5A-AF5365D062CE"),
                UserName = "alaoui.o@gmail.com",
                calls_free = 1000,
                CurrentActive = "active"
            };

            // Save to Serialization
            String path = Server.MapPath("~/App_Data/data_M_TWINGLYACCOUNT.txt");
            new TextPersist().Serialize<M_TWINGLYACCOUNT>(contactATallpecmediaDOTcom, path);
            new TextPersist().Serialize<M_TWINGLYACCOUNT>(alaouiDOToATgmailDOTcom, path);

            //
            // return View("Index");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult TwinglySetup_changeActiveAccount(Guid newlyActive_id_twinglyaccount_api_key)
        {
            // deserialize & send twingly accounts
            // var twinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));
            List<M_TWINGLYACCOUNT> twinglyAccounts = loaddeserializeM_TWINGLYACCOUNT_DAPPERSQL();

            // disable active one
            twinglyAccounts.Find(m => m.CurrentActive == "active").CurrentActive = String.Empty;

            // active the selected one
            twinglyAccounts.Find(m => m.ID_TWINGLYACCOUNT_API_KEY == newlyActive_id_twinglyaccount_api_key).CurrentActive = "active";

            // Save back to Serialization
            new TextPersist().SerializeBack_dataPath<M_TWINGLYACCOUNT>(twinglyAccounts, Server.MapPath("~/App_Data"));

            //
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult TwinglySetup_AddNewActiveAccount(Guid newlyActive_id_twinglyaccount_api_key)
        {
            // deserialize & send twingly accounts
            // var twinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));
            List<M_TWINGLYACCOUNT> twinglyAccounts = loaddeserializeM_TWINGLYACCOUNT_DAPPERSQL();

            // disable active one
            twinglyAccounts.Find(m => m.CurrentActive == "active").CurrentActive = String.Empty;

            //
            twinglyAccounts.Add(new M_TWINGLYACCOUNT
            {
                ID_TWINGLYACCOUNT_API_KEY = newlyActive_id_twinglyaccount_api_key,
                UserName = "",
                calls_free = 1000,
                CurrentActive = "active"
            });

            // Save back to Serialization
            new TextPersist().SerializeBack_dataPath<M_TWINGLYACCOUNT>(twinglyAccounts, Server.MapPath("~/App_Data"));

            //
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult TwinglySetup_deleteAccount(Guid id_twinglyaccount_api_key)
        {
            // deserialize & send twingly accounts
            // var twinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));
            List<M_TWINGLYACCOUNT> twinglyAccounts = loaddeserializeM_TWINGLYACCOUNT_DAPPERSQL();

            // find the one to delete
            var accountToDelete = twinglyAccounts.Find(m => m.ID_TWINGLYACCOUNT_API_KEY == id_twinglyaccount_api_key);

            // disable active one
            var activeAccount = twinglyAccounts.Find(m => m.CurrentActive == "active");

            // we can not delete active account
            if (accountToDelete == activeAccount)
            {
                TempData["showAlertWarning"] = true;
                TempData["msgAlert"] = "you cannot delete the active Twingly account";
                return RedirectToAction("Index");
            }

            // remove
            twinglyAccounts.Remove(accountToDelete);

            // Save back to Serialization
            new TextPersist().SerializeBack_dataPath<M_TWINGLYACCOUNT>(twinglyAccounts, Server.MapPath("~/App_Data"));

            //
            return RedirectToAction("Index");
        }
        #endregion

        #region FRONT YARD ACTIONS THEME
        [HttpPost]
        public ActionResult XtrctTheme_AddNew(String themename, String themetags)
        {
            // create the theme
            var newXtrctTheme = new M_XTRCTTHEME
            {
                ID_XTRCTTHEME = Guid.NewGuid(),
                ThemeName = themename.Trim()
            };

            // Save to Serialization
            var path = Server.MapPath("~/App_Data/data_M_XTRCTTHEME.txt");
            new TextPersist().Serialize(newXtrctTheme, path);

            // create the associated tags
            foreach (var themetag in themetags.Split(new char[] { ',' }))
            {
                var newXrtctThemeKeyword = new M_XTRCTTHEME_KEYWORD
                {
                    ID_XTRCTTHEME_KEYWORD = Guid.NewGuid(),
                    ID_XTRCTTHEME = newXtrctTheme.ID_XTRCTTHEME,
                    Keyword = themetag
                };

                // Save to Serialization
                path = Server.MapPath("~/App_Data/data_M_XTRCTTHEME_KEYWORD.txt");
                new TextPersist().Serialize(newXrtctThemeKeyword, path);
            }

            //
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult XtrctTheme_ApplyNewActive(String themename)
        {
            // find previous active, and disable it
            saveserializeM_XTRCTTHEME_EFSQL_Deactivate();

            // find to-be-active by name, and make it active
            saveserializeM_XTRCTTHEME_EFSQL_Active(themename);

            //
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult XtrctTheme_Keywords_Reload(String themename)
        {
            // List<Tuple<String, int>> tagscounts = loadDeserializeM_ARABICDARIJAENTRY_TEXTENTITY_THEMETAGSCOUNT_DAPPERSQL(themename);
            List<THEMETAGSCOUNT> tagscounts = loadDeserializeM_ARABICDARIJAENTRY_TEXTENTITY_THEMETAGSCOUNT_DAPPERSQL(themename);

            //
            var activeXtrctTheme = loadDeserializeM_XTRCTTHEME_Active_DAPPERSQL();

            //
            List<M_XTRCTTHEME_KEYWORD> xtrctThemesKeywords = new List<M_XTRCTTHEME_KEYWORD>();
            foreach (var tagcount in tagscounts)
            {
                xtrctThemesKeywords.Add(new M_XTRCTTHEME_KEYWORD
                {
                    ID_XTRCTTHEME_KEYWORD = Guid.NewGuid(),
                    ID_XTRCTTHEME = activeXtrctTheme.ID_XTRCTTHEME,
                    Keyword = tagcount.TextEntity_Mention
                });
            }

            // save
            saveserializeM_XTRCTTHEME_KEYWORDs_EFSQL(xtrctThemesKeywords, activeXtrctTheme);

            //
            return RedirectToAction("Index");
        }
        #endregion

        // This action handles the form POST and the upload
        [HttpPost]
        public ActionResult Data_Upload(HttpPostedFileBase file, String mainEntity)
        {
            Logging.Write(Server, "Data_Upload - 1");

            // check before if the user selected a file
            if (file == null || file.ContentLength == 0)
            {
                // we use tempdata instead of viewbag because viewbag can't be passed over to a controller
                TempData["showAlertWarning"] = true;
                TempData["msgAlert"] = "No file has been chosen.";
                return RedirectToAction("Index");
            }

            Logging.Write(Server, "Data_Upload - 2");

            // check before if mainEntity is not empty
            if (String.IsNullOrWhiteSpace(mainEntity))
            {
                // we use tempdata instead of viewbag because viewbag can't be passed over to a controller
                TempData["showAlertWarning"] = true;
                TempData["msgAlert"] = "No main entity has been entered.";
                return RedirectToAction("Index");
            }

            // extract only the filename
            var fileName = Path.GetFileName(file.FileName);

            // store the file inside ~/App_Data/uploads folder
            var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
            file.SaveAs(path);

            var token = Session["_T0k@n_"];
            string errMessage = string.Empty;
            if (token != null && _IAuthenticate.IsTokenValid(Convert.ToString(token), "Data_Upload", out errMessage))
            {
                // loop and process each one
                var lines = System.IO.File.ReadLines(path).ToList();
                foreach (string line in lines)
                {
                    train(new M_ARABIZIENTRY
                    {
                        ArabiziText = line.Trim(),
                        ArabiziEntryDate = DateTime.Now
                    }, mainEntity);
                }

                // mark how many rows been translated
                TempData["showAlertSuccess"] = true;
                TempData["msgAlert"] = lines.Count.ToString() + " rows has been imported.";
            }
            else
            {
                Session["_T0k@n_"] = "";
                TempData["showAlertWarning"] = true;
                TempData["msgAlert"] = errMessage;  // "Not a valid token";
            }

            // delete just uploaded file in uploads
            System.IO.File.Delete(path);

            // redirect back to the index action to show the form once again
            return RedirectToAction("Index");
        }

        [HttpPost]
        public void Log(String message)
        {
            Logging.Write(Server, message);
        }

        public object DataTablesNet_ServerSide_GetList()
        {
            try
            {
                //
                int start = 0;
                int itemsPerPage = 10;

                // get from client side, from where we start the paging
                int.TryParse(this.Request.QueryString["start"], out start);

                // get from client side, to which length the paging goes
                int.TryParse(this.Request.QueryString["length"], out itemsPerPage);

                // get from client search word
                string searchValue = this.Request.QueryString["search[value]"].ToString();

                //
                string searchAccount = this.Request.QueryString["columns[0][search][value]"];
                string searchSource = this.Request.QueryString["columns[1][search][value]"];
                string searchEntity = this.Request.QueryString["columns[2][search][value]"];
                string searchName = this.Request.QueryString["columns[3][search][value]"];

                // get main (whole) data from DB first
                List<ArabiziToArabicViewModel> items = loadArabiziToArabicViewModel_DAPPERSQL(activeThemeOnly: true);

                // get the number of entries
                var itemsCount = items.Count;

                // adjust itemsPerPage case show all
                if (itemsPerPage == -1)
                    itemsPerPage = itemsCount;

                // filter on search term if any
                if (!String.IsNullOrEmpty(searchValue))
                {
                    items = items.Where(a => a.ArabiziText.ToUpper().Contains(searchValue.ToUpper()) || a.ArabicDarijaText.ToUpper().Contains(searchValue.ToUpper())).ToList();
                }

                // page as per request (index of page and length)
                items = items.Skip(start).Take(itemsPerPage).ToList();

                // get other helper data from DB
                List<M_ARABICDARIJAENTRY_LATINWORD> arabicDarijaEntryLatinWords = loaddeserializeM_ARABICDARIJAENTRY_LATINWORD_DAPPERSQL();
                List<M_ARABICDARIJAENTRY_TEXTENTITY> TextEntities = loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY_DAPPERSQL();
                List<M_XTRCTTHEME> MainEntities = loaddeserializeM_XTRCTTHEME_DAPPERSQL();

                // Visual formatting before sending back
                items.ForEach(s =>
                {
                    s.PositionHash = itemsCount - start - items.IndexOf(s);
                    s.FormattedArabiziEntryDate = s.ArabiziEntryDate.ToString("yyyy-MM-dd HH:mm");
                    s.FormattedArabicDarijaText = TextTools.HighlightExtractedLatinWords(s.ArabicDarijaText, s.ID_ARABICDARIJAENTRY, arabicDarijaEntryLatinWords);
                    s.FormattedEntitiesTypes = TextTools.DisplayEntitiesType(s.ID_ARABICDARIJAENTRY, TextEntities);
                    s.FormattedEntities = TextTools.DisplayEntities(s.ID_ARABICDARIJAENTRY, TextEntities);
                    s.FormattedRemoveAndApplyTagCol = TextTools.DisplayRemoveAndApplyTagCol(s.ID_ARABIZIENTRY, s.ID_ARABICDARIJAENTRY, MainEntities);
                });

                //
                return JsonConvert.SerializeObject(new
                {
                    recordsTotal = itemsCount.ToString(),
                    recordsFiltered = itemsCount.ToString(),
                    data = items
                });
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return null;
            }
        }

        public object DataTablesNet_ServerSide_FB_GetList(string fluencerid)
        {
            // get main (whole) data from DB first
            var items = loaddeserializeT_FB_POST_DAPPERSQL(fluencerid).Select(c => new
            {
                id = c.id,
                fk_i = c.fk_influencer,
                pt = c.post_text,
                tt = c.translated_text,
                lc = c.likes_count,
                cc = c.comments_count,
                dp = c.date_publishing
            }).ToList();
            // get the number of entries
            var itemsCount = items.Count;

            //
            return JsonConvert.SerializeObject(new
            {
                recordsTotal = itemsCount.ToString(),
                recordsFiltered = itemsCount.ToString(),
                data = items
            });
        }

        public object GetFBPostComment(string id)
        {
            var items = new List<FBFeedComment>();
            var itemsCount = 0;
            if (!string.IsNullOrEmpty(id))
            {
                //var idForPost = id.Split('_')[1];
                items = loaddeserializeT_FB_Comments(id);
                itemsCount = items.Count;
            }
            //
            return JsonConvert.SerializeObject(new
            {
                recordsTotal = itemsCount.ToString(),
                recordsFiltered = itemsCount.ToString(),
                data = items
            });
        }

        #region FRONT YARD ACTIONS SETUP DB
        [HttpGet]
        public void SetupCreateFillDBFromXML()
        {
            // Warning this methiod has to be executed (on VM and on server) once to make the swicth between XML serialization to DB. Otherwise any new entry 
            // to DB will be erased by the ones previously in xml

            using (var db = new ArabiziDbContext())
            {
                var M_TWINGLYACCOUNTs = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));
                var M_ARABICDARIJAENTRYs = new TextPersist().Deserialize<M_ARABICDARIJAENTRY>(Server.MapPath("~/App_Data"));
                var M_ARABICDARIJAENTRY_LATINWORDs = new TextPersist().Deserialize<M_ARABICDARIJAENTRY_LATINWORD>(Server.MapPath("~/App_Data"));
                var M_ARABICDARIJAENTRY_TEXTENTITYs = new TextPersist().Deserialize<M_ARABICDARIJAENTRY_TEXTENTITY>(Server.MapPath("~/App_Data"));
                var M_ARABIZIENTRYs = new TextPersist().Deserialize<M_ARABIZIENTRY>(Server.MapPath("~/App_Data"));
                var M_XTRCTTHEMEs = new TextPersist().Deserialize<M_XTRCTTHEME>(Server.MapPath("~/App_Data"));
                var M_XTRCTTHEME_KEYWORDs = new TextPersist().Deserialize<M_XTRCTTHEME_KEYWORD>(Server.MapPath("~/App_Data"));

                // clean too early date => 2017-01-01 00:00:00 (DateTime.MinValue)
                M_ARABIZIENTRYs.Where(w => w.ArabiziEntryDate == DateTime.MinValue).ToList().ForEach(s => s.ArabiziEntryDate = DateTime.Now.AddYears(-1));

                //
                db.M_TWINGLYACCOUNTs.AddRange(M_TWINGLYACCOUNTs);
                db.M_ARABICDARIJAENTRYs.AddRange(M_ARABICDARIJAENTRYs);
                db.M_ARABICDARIJAENTRY_LATINWORDs.AddRange(M_ARABICDARIJAENTRY_LATINWORDs);
                db.M_ARABICDARIJAENTRY_TEXTENTITYs.AddRange(M_ARABICDARIJAENTRY_TEXTENTITYs);
                db.M_ARABIZIENTRYs.AddRange(M_ARABIZIENTRYs);
                db.M_XTRCTTHEMEs.AddRange(M_XTRCTTHEMEs);
                db.M_XTRCTTHEME_KEYWORDs.AddRange(M_XTRCTTHEME_KEYWORDs);

                // commit
                db.SaveChanges();
            }
        }

        [HttpGet]
        public void SetupDeleteDB()
        {
            using (var db = new ArabiziDbContext())
            {
                db.Database.Delete();
            }
        }
        #endregion

        // MC112917 TMP COMMENT SO IT CAN COMPILE
        /*[HttpPost]
        public ActionResult FetchFBData(Search search, int id)
        {
            @ViewBag.Message = "";
            string Error = string.Empty;

            //
            var fbApp = clBusiness.GetFbApplication(id);

            //
            search.FbAccessToken = clBusiness.FacebookGetAccessToken(fbApp);

            //
            clBusiness.getFacebookGroupFeed(search, fbApp, ref Error);

            //
            if (string.IsNullOrEmpty(Error))
                return RedirectToAction("Index", "Home");
            else
            {
                @ViewBag.Message = Error;
                return View(search);
            }
        }*/

        #region BACK YARD BO TRAIN
        private Guid train(M_ARABIZIENTRY arabiziEntry, String mainEntity)
        {
            // Arabizi to arabic from perl script
            if (arabiziEntry.ArabiziText == null)
                return Guid.Empty;

            //
            var id_ARABICDARIJAENTRY = Guid.NewGuid();

            Logging.Write(Server, "train - before lock : 0");
            var watch = Stopwatch.StartNew();
            lock (thisLock)
            {
                Logging.Write(Server, "train - after lock (thisLock) : " + watch.ElapsedMilliseconds); // watch.Restart();

                String arabicText = train_savearabizi(arabiziEntry, AccessMode.efsql);
                Logging.Write(Server, "train - after train_savearabizi : " + watch.ElapsedMilliseconds); // watch.Restart();

                //
                arabicText = new TextConverter().Preprocess_upstream(arabicText);
                Logging.Write(Server, "train - after train_Preprocess_upstream : " + watch.ElapsedMilliseconds); // watch.Restart();

                arabicText = train_bidict(arabicText);
                Logging.Write(Server, "train - after train_bidict : " + watch.ElapsedMilliseconds); // watch.Restart();

                arabicText = train_binggoogle(arabicText);
                Logging.Write(Server, "train - after train_binggoogle : " + watch.ElapsedMilliseconds); // watch.Restart();

                arabicText = train_saveperl(watch, arabicText, arabiziEntry.ID_ARABIZIENTRY, id_ARABICDARIJAENTRY, AccessMode.efsql);
                Logging.Write(Server, "train - after train_saveperl : " + watch.ElapsedMilliseconds); // watch.Restart();

                arabicText = train_savelatinwords(arabicText, id_ARABICDARIJAENTRY, AccessMode.efsql);
                Logging.Write(Server, "train - after train_savelatinwords : " + watch.ElapsedMilliseconds); // watch.Restart();

                train_savesa(arabicText);
                Logging.Write(Server, "train - after train_savesa : " + watch.ElapsedMilliseconds); // watch.Restart();

                train_savener(arabicText, id_ARABICDARIJAENTRY, AccessMode.efsql);
                Logging.Write(Server, "train - after train_savener : " + watch.ElapsedMilliseconds); // watch.Restart();

                // apply main tag : add main entity & Save to Serialization
                var textEntity = new M_ARABICDARIJAENTRY_TEXTENTITY
                {
                    ID_ARABICDARIJAENTRY_TEXTENTITY = Guid.NewGuid(),
                    ID_ARABICDARIJAENTRY = id_ARABICDARIJAENTRY,
                    TextEntity = new TextEntity
                    {
                        Count = 1,
                        Mention = mainEntity,
                        Type = "MAIN ENTITY"
                    }
                };
                saveserializeM_ARABICDARIJAENTRY_TEXTENTITY_EFSQL(textEntity);
            }
            Logging.Write(Server, "train - after end of lock : " + watch.ElapsedMilliseconds);

            //
            return id_ARABICDARIJAENTRY;
        }

        private String train_savearabizi(M_ARABIZIENTRY arabiziEntry, AccessMode accessMode)
        {
            // complete arabizi entry & Save arabiziEntry to Serialization
            arabiziEntry.ID_ARABIZIENTRY = Guid.NewGuid();

            // clean
            arabiziEntry.ArabiziText = arabiziEntry.ArabiziText.Trim(new char[] { ' ', '\t' });

            // save
            saveserializeM_ARABIZIENTRY(arabiziEntry, accessMode);

            return arabiziEntry.ArabiziText;
        }

        private static string train_bidict(string arabicText)
        {
            // clean before google/bing : o w
            arabicText = new TextFrequency().ReplaceArzByArFromBidict(arabicText);
            return arabicText;
        }

        private static string train_binggoogle(String arabicText)
        {
            // first pass : correct/translate the original arabizi into msa arabic using big/google apis (to take care of french/english segments in codeswitch arabizi posts)
            return new TranslationTools().CorrectTranslate(arabicText);
        }

        private String train_saveperl(Stopwatch watch, string arabicText, Guid id_ARABIZIENTRY, Guid id_ARABICDARIJAENTRY, AccessMode accessMode)
        {
            // first process buttranslateperl : means those should be cleaned in their without-bracket origan form so they can be translated by perl
            // ex : "<span class='notranslate BUTTRANSLATEPERL'>kolchi</span> katbakkih bl3ani" should become "kolchi katbakkih bl3ani" so perl can translate it
            arabicText = new Regex(@"<span class='notranslate BUTTRANSLATEPERL'>(.*?)</span>").Replace(arabicText, "$1");

            // process notranslate
            var regex = new Regex(@"<span class='notranslate'>(.*?)</span>");

            // save matches
            var matches = regex.Matches(arabicText);

            // skip over non-translantable parts
            arabicText = regex.Replace(arabicText, "001000100");

            // translate arabizi to darija arabic script using perl script via direct call and save arabicDarijaEntry to Serialization
            Logging.Write(Server, "train - before train_saveperl > Convert : " + watch.ElapsedMilliseconds);
            arabicText = new TextConverter().Convert(Server, watch, arabicText);
            Logging.Write(Server, "train - after train_saveperl > Convert : " + watch.ElapsedMilliseconds);

            // restore do not translate from 001000100
            var regex2 = new Regex(@"001000100");
            foreach (Match match in matches)
            {
                arabicText = regex2.Replace(arabicText, match.Value, 1);
            }
            // clean <span class='notranslate'>
            arabicText = new Regex(@"<span class='notranslate'>(.*?)</span>").Replace(arabicText, "$1");

            //
            var arabicDarijaEntry = new M_ARABICDARIJAENTRY
            {
                ID_ARABICDARIJAENTRY = id_ARABICDARIJAENTRY,
                ID_ARABIZIENTRY = id_ARABIZIENTRY,
                ArabicDarijaText = arabicText
            };

            // save to persist
            saveserializeM_ARABICDARIJAENTRY(arabicDarijaEntry, accessMode);

            //
            return arabicText;
        }

        private String train_savelatinwords(String arabicText, Guid id_ARABICDARIJAENTRY, AccessMode accessMode)
        {
            // extract latin words and save every match of latin words
            // also calculate on the fly the number of variants
            MatchCollection matches = TextTools.ExtractLatinWords(arabicText);
            foreach (Match match in matches)
            {
                // do not consider words in the bidict as latin words
                if (new TextFrequency().BidictContainsWord(match.Value))
                    continue;

                // count variants
                String arabiziWord = match.Value;
                int variantsCount = new TextConverter().GetAllTranscriptions(arabiziWord).Count;

                var latinWord = new M_ARABICDARIJAENTRY_LATINWORD
                {
                    ID_ARABICDARIJAENTRY_LATINWORD = Guid.NewGuid(),
                    ID_ARABICDARIJAENTRY = id_ARABICDARIJAENTRY,
                    LatinWord = arabiziWord,
                    VariantsCount = variantsCount
                };

                // Save to Serialization
                saveserializeM_ARABICDARIJAENTRY_LATINWORD(latinWord, accessMode);
            }

            return arabicText;
        }

        private void train_savesa(string arabicText)
        {
            // Sentiment analysis from watson https://gateway.watsonplatform.net/" and Save to Serialization
            var textSentimentAnalyzer = new TextSentimentAnalyzer();
            TextSentiment sentiment = textSentimentAnalyzer.GetSentiment(arabicText);

            //
            saveserializeM_ARABICDARIJAENTRY_LATINWORD_XML(sentiment);
        }

        private void train_savener(string arabicText, Guid id_ARABICDARIJAENTRY, AccessMode accessMode)
        {
            // Entity extraction from rosette (https://api.rosette.com/rest/v1/)
            var entities = new TextEntityExtraction().GetEntities(arabicText);

            // NER manual extraction
            new TextEntityExtraction().NerManualExtraction(arabicText, entities, id_ARABICDARIJAENTRY, Server, saveserializeM_ARABICDARIJAENTRY_TEXTENTITY, accessMode);
        }
        #endregion

        #region BACK YARD BO LOAD
        private List<M_ARABICDARIJAENTRY> loaddeserializeM_ARABICDARIJAENTRY(AccessMode accessMode)
        {
            if (accessMode == AccessMode.xml)
                return loaddeserializeM_ARABICDARIJAENTRY();
            else if (accessMode == AccessMode.efsql)
                return loaddeserializeM_ARABICDARIJAENTRY_DB();
            else if (accessMode == AccessMode.dappersql)
                return loaddeserializeM_ARABICDARIJAENTRY_DAPPERSQL();

            return null;
        }

        private List<M_ARABICDARIJAENTRY> loaddeserializeM_ARABICDARIJAENTRY()
        {
            List<M_ARABICDARIJAENTRY> entries = new List<M_ARABICDARIJAENTRY>();
            string path = Server.MapPath("~/App_Data/data_" + typeof(M_ARABICDARIJAENTRY).Name + ".txt");
            XmlSerializer serializer = new XmlSerializer(entries.GetType());
            using (var reader = new System.IO.StreamReader(path))
            {
                entries = (List<M_ARABICDARIJAENTRY>)serializer.Deserialize(reader);
            }

            return entries;
        }

        private List<M_ARABICDARIJAENTRY> loaddeserializeM_ARABICDARIJAENTRY_DB()
        {
            using (var db = new ArabiziDbContext())
            {
                return db.M_ARABICDARIJAENTRYs.ToList();
            }
        }

        private List<M_ARABICDARIJAENTRY> loaddeserializeM_ARABICDARIJAENTRY_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_ARABICDARIJAENTRY ";

                conn.Open();
                return conn.Query<M_ARABICDARIJAENTRY>(qry).ToList();
            }
        }

        private List<M_ARABICDARIJAENTRY_TEXTENTITY> loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY(AccessMode accessMode)
        {
            if (accessMode == AccessMode.xml)
                return loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY();
            else if (accessMode == AccessMode.efsql)
                return loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY_DB();
            else if (accessMode == AccessMode.dappersql)
                return loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY_DAPPERSQL();

            return null;
        }

        private List<M_ARABICDARIJAENTRY_TEXTENTITY> loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY()
        {
            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = new List<M_ARABICDARIJAENTRY_TEXTENTITY>();
            string path = Server.MapPath("~/App_Data/data_" + typeof(M_ARABICDARIJAENTRY_TEXTENTITY).Name + ".txt");
            XmlSerializer serializer = new XmlSerializer(textEntities.GetType());
            if (System.IO.File.Exists(path))
            {
                using (var reader = new System.IO.StreamReader(path))
                {
                    textEntities = (List<M_ARABICDARIJAENTRY_TEXTENTITY>)serializer.Deserialize(reader);
                }
            }

            return textEntities;
        }

        private List<M_ARABICDARIJAENTRY_TEXTENTITY> loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY_DB()
        {
            using (var db = new ArabiziDbContext())
            {
                return db.M_ARABICDARIJAENTRY_TEXTENTITYs.ToList();
            }
        }

        private List<M_ARABICDARIJAENTRY_TEXTENTITY> loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_ARABICDARIJAENTRY_TEXTENTITY";

                conn.Open();

                // special case, our initial class was complex (deep TextEntity), DB table created from initial class by EF is somehow flat
                // so at loading, we need to convert back from flat to complex deep
                var flats = conn.Query<M_ARABICDARIJAENTRY_TEXTENTITY_FLAT>(qry);

                //
                List<M_ARABICDARIJAENTRY_TEXTENTITY> unflats = new List<M_ARABICDARIJAENTRY_TEXTENTITY>();
                foreach (var flat in flats)
                {
                    M_ARABICDARIJAENTRY_TEXTENTITY unflat = new M_ARABICDARIJAENTRY_TEXTENTITY
                    {
                        ID_ARABICDARIJAENTRY = flat.ID_ARABICDARIJAENTRY,
                        ID_ARABICDARIJAENTRY_TEXTENTITY = flat.ID_ARABICDARIJAENTRY_TEXTENTITY,
                        TextEntity = new TextEntity
                        {
                            Count = flat.TextEntity_Count,
                            EntityId = flat.TextEntity_EntityId,
                            Mention = flat.TextEntity_Mention,
                            Normalized = flat.TextEntity_Normalized,
                            Type = flat.TextEntity_Type

                        }
                    };
                    unflats.Add(unflat);
                }

                return unflats;
                // return conn.Query<M_ARABICDARIJAENTRY_TEXTENTITY>(qry).ToList();
            }
        }

        // private List<Tuple<String, int>> loadDeserializeM_ARABICDARIJAENTRY_TEXTENTITY_THEMETAGSCOUNT_DAPPERSQL(String themename)
        private List<THEMETAGSCOUNT> loadDeserializeM_ARABICDARIJAENTRY_TEXTENTITY_THEMETAGSCOUNT_DAPPERSQL(String themename)
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            // anti sql injection
            themename = themename.Replace("'", "''").Trim();

            //
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry0 = "SELECT "
                                + "TextEntity_Mention, SUM(TextEntity_Count) SUM_TextEntity_Count "
                            + "FROM T_ARABICDARIJAENTRY_TEXTENTITY "
                            + "WHERE ID_ARABICDARIJAENTRY IN( "
                                + "SELECT ID_ARABICDARIJAENTRY "
                                + "FROM T_ARABICDARIJAENTRY_TEXTENTITY "
                                + "WHERE TextEntity_Type = 'MAIN ENTITY' "
                                + "AND TextEntity_Mention like '" + themename + "' "
                            + ") "
                            + "AND TextEntity_Type != 'MAIN ENTITY' "
                            + "AND TextEntity_Type != 'PREPOSITION' "
                            + "AND TextEntity_Type != 'PRONOUN' "
                            + "GROUP BY TextEntity_Mention ";

                conn.Open();
                // return conn.Query<Tuple<String, int>>(qry0).ToList();
                return conn.Query<THEMETAGSCOUNT>(qry0).ToList();
            }
        }

        private List<M_ARABIZIENTRY> loaddeserializeM_ARABIZIENTRY(AccessMode accessMode)
        {
            if (accessMode == AccessMode.xml)
                return loaddeserializeM_ARABIZIENTRY();
            else if (accessMode == AccessMode.efsql)
                return loaddeserializeM_ARABIZIENTRY_DB();
            else if (accessMode == AccessMode.dappersql)
                return loaddeserializeM_ARABIZIENTRY_DAPPERSQL();

            return null;
        }

        private List<M_ARABIZIENTRY> loaddeserializeM_ARABIZIENTRY()
        {
            List<M_ARABIZIENTRY> arabiziEntries = new List<M_ARABIZIENTRY>();
            string path = Server.MapPath("~/App_Data/data_" + typeof(M_ARABIZIENTRY).Name + ".txt");
            XmlSerializer serializer = new XmlSerializer(arabiziEntries.GetType());
            using (var reader = new System.IO.StreamReader(path))
            {
                arabiziEntries = (List<M_ARABIZIENTRY>)serializer.Deserialize(reader);
            }

            return arabiziEntries;
        }

        private List<M_ARABIZIENTRY> loaddeserializeM_ARABIZIENTRY_DB()
        {
            using (var db = new ArabiziDbContext())
            {
                return db.M_ARABIZIENTRYs.ToList();
            }
        }

        private List<M_ARABIZIENTRY> loaddeserializeM_ARABIZIENTRY_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_ARABIZIENTRY";

                conn.Open();
                return conn.Query<M_ARABIZIENTRY>(qry).ToList();
            }
        }

        private List<M_ARABICDARIJAENTRY_LATINWORD> loaddeserializeM_ARABICDARIJAENTRY_LATINWORD(AccessMode accessMode)
        {
            if (accessMode == AccessMode.xml)
                return loaddeserializeM_ARABICDARIJAENTRY_LATINWORD();
            else if (accessMode == AccessMode.efsql)
                return loaddeserializeM_ARABICDARIJAENTRY_LATINWORD_DB();
            else if (accessMode == AccessMode.dappersql)
                return loaddeserializeM_ARABICDARIJAENTRY_LATINWORD_DAPPERSQL();

            return null;
        }

        private List<M_ARABICDARIJAENTRY_LATINWORD> loaddeserializeM_ARABICDARIJAENTRY_LATINWORD()
        {
            List<M_ARABICDARIJAENTRY_LATINWORD> latinWordsEntries = new List<M_ARABICDARIJAENTRY_LATINWORD>();
            string path = Server.MapPath("~/App_Data/data_" + typeof(M_ARABICDARIJAENTRY_LATINWORD).Name + ".txt");
            XmlSerializer serializer = new XmlSerializer(latinWordsEntries.GetType());
            using (var reader = new System.IO.StreamReader(path))
            {
                latinWordsEntries = (List<M_ARABICDARIJAENTRY_LATINWORD>)serializer.Deserialize(reader);
            }

            return latinWordsEntries;
        }

        private List<M_ARABICDARIJAENTRY_LATINWORD> loaddeserializeM_ARABICDARIJAENTRY_LATINWORD_DB()
        {
            using (var db = new ArabiziDbContext())
            {
                return db.M_ARABICDARIJAENTRY_LATINWORDs.ToList();
            }
        }

        private List<M_ARABICDARIJAENTRY_LATINWORD> loaddeserializeM_ARABICDARIJAENTRY_LATINWORD_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_ARABICDARIJAENTRY_LATINWORD";

                conn.Open();
                return conn.Query<M_ARABICDARIJAENTRY_LATINWORD>(qry).ToList();
            }
        }

        private List<M_XTRCTTHEME> loaddeserializeM_XTRCTTHEME_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_XTRCTTHEME ORDER BY ThemeName ";

                conn.Open();
                return conn.Query<M_XTRCTTHEME>(qry).ToList();
            }
        }

        private M_XTRCTTHEME loadDeserializeM_XTRCTTHEME_Active_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_XTRCTTHEME WHERE CurrentActive = 'active' ";

                conn.Open();
                return conn.QueryFirst<M_XTRCTTHEME>(qry);
            }
        }

        private List<M_XTRCTTHEME_KEYWORD> loaddeserializeM_XTRCTTHEME_KEYWORD_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_XTRCTTHEME_KEYWORD";

                conn.Open();
                return conn.Query<M_XTRCTTHEME_KEYWORD>(qry).ToList();
            }
        }

        private List<M_XTRCTTHEME_KEYWORD> loaddeserializeM_XTRCTTHEME_KEYWORD_Active_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_XTRCTTHEME_KEYWORD XK INNER JOIN T_XTRCTTHEME X ON XK.ID_XTRCTTHEME = X.ID_XTRCTTHEME AND X.CurrentActive = 'active' ";

                conn.Open();
                return conn.Query<M_XTRCTTHEME_KEYWORD>(qry).ToList();
            }
        }

        private List<M_TWINGLYACCOUNT> loaddeserializeM_TWINGLYACCOUNT_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_TWINGLYACCOUNT";

                conn.Open();
                return conn.Query<M_TWINGLYACCOUNT>(qry).ToList();
            }
        }

        private List<FB_POST> loaddeserializeT_FB_POST_DAPPERSQL(string influencerid = "")
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "";
                if (!string.IsNullOrEmpty(influencerid))
                {
                    qry = "SELECT * FROM T_FB_POST where fk_influencer='" + influencerid + "'";
                }
                else
                {
                    qry = "SELECT * FROM T_FB_POST";
                }


                conn.Open();
                return conn.Query<FB_POST>(qry).ToList();
            }
        }
        private List<FBFeedComment> loaddeserializeT_FB_Comments(string postid = "")
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "";
                if (!string.IsNullOrEmpty(postid))
                {
                    qry = "select * from FBFeedComments where id like '" + postid + "%'";
                }
                //else
                //{
                //    qry = "SELECT * FROM FBFeedComments";
                //}


                conn.Open();
                return conn.Query<FBFeedComment>(qry).ToList();
            }
        }
        private List<FBFeedComment> GetComments(string ids)
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "";
                if (!string.IsNullOrEmpty(ids))
                {
                    qry = "select * from FBFeedComments where id in (" + ids + ")and translated_message is null";
                }
                //else
                //{
                //    qry = "SELECT * FROM FBFeedComments";
                //}


                conn.Open();
                return conn.Query<FBFeedComment>(qry).ToList();
            }
        }

        private int SaveTranslatedPost(string postid, string TranslatedText)
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;
            int returndata = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    String qry = "";
                    if (!string.IsNullOrEmpty(postid) && !string.IsNullOrEmpty(TranslatedText))
                    {
                        qry = "update T_FB_POST set translated_text=N'" + TranslatedText + "' where id='" + postid + "'";
                    }
                    using (SqlCommand cmd = new SqlCommand(qry, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        returndata = cmd.ExecuteNonQuery();
                        conn.Close();
                    }

                }
            }
            catch (Exception e)
            {
                returndata = 0;
            }
            return returndata;
        }

        private int SaveTranslatedComments(string postid, string TranslatedText)
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;
            int returndata = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    String qry = "";
                    if (!string.IsNullOrEmpty(postid) && !string.IsNullOrEmpty(TranslatedText))
                    {
                        qry = "update FBFeedComments set translated_message=N'" + TranslatedText + "' where id='" + postid + "'";
                    }
                    using (SqlCommand cmd = new SqlCommand(qry, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        conn.Open();
                        returndata = cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    // conn.Open();

                    // return conn.Query<FB_POST>(qry).ToList();
                }
            }
            catch (Exception e)
            {
                returndata = 0;
            }
            return returndata;

        }

        //public Guid GetActiveThemeId()
        //{          
        //    using (var db = new ArabiziDbContext())
        //    {
        //        var theme = db.M_XTRCTTHEMEs.FirstOrDefault(c => c.CurrentActive == "active");
        //        if (theme != null)
        //        {
        //            var result= theme.ID_XTRCTTHEME;
        //            return result;
        //        }
        //        else
        //        {
        //            return new Guid();
        //        }
        //    }

        //}

        private List<T_FB_INFLUENCER> loadAllT_Fb_InfluencerAsTheme(string themeid = "")
        {
            var t_fb_Influencer = new List<T_FB_INFLUENCER>();
            var themes = loadDeserializeM_XTRCTTHEME_Active_DAPPERSQL();
            M_XTRCTTHEME theme = (themes != null) ? themes : new M_XTRCTTHEME();
            if (theme.ID_XTRCTTHEME != null)
            {
                String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    String qry = "SELECT * FROM T_FB_INFLUENCER where fk_theme='" + theme.ID_XTRCTTHEME + "'";

                    conn.Open();
                    return conn.Query<T_FB_INFLUENCER>(qry).ToList();
                }
            }
            else
            {
                return t_fb_Influencer;
            }


        }

        private List<ArabiziToArabicViewModel> loadArabiziToArabicViewModel_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT "
                        + "ARZ.ID_ARABIZIENTRY, "
                        + "ARZ.ArabiziEntryDate, "
                        + "ARZ.ArabiziText, "
                        + "AR.ID_ARABICDARIJAENTRY, "
                        + "AR.ArabicDarijaText "
                    + "FROM T_ARABIZIENTRY ARZ "
                    + "INNER JOIN T_ARABICDARIJAENTRY AR ON ARZ.ID_ARABIZIENTRY = AR.ID_ARABIZIENTRY "
                    + "ORDER BY ARZ.ArabiziEntryDate DESC ";

                conn.Open();
                return conn.Query<ArabiziToArabicViewModel>(qry).ToList();
            }
        }

        /*private List<ArabiziToArabicViewModel2> loadArabiziToArabicViewModel2_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT "
                        + "ARZ.ID_ARABIZIENTRY, "
                        + "ARZ.ArabiziEntryDate, "
                        + "ARZ.ArabiziText, "
                        + "AR.ID_ARABICDARIJAENTRY, "
                        + "AR.ArabicDarijaText, "
                        + "ARTE.TextEntity_Mention "
                    + "FROM T_ARABIZIENTRY ARZ "
                    + "INNER JOIN T_ARABICDARIJAENTRY AR ON ARZ.ID_ARABIZIENTRY = AR.ID_ARABIZIENTRY "
                    + "INNER JOIN T_ARABICDARIJAENTRY_TEXTENTITY ARTE ON AR.ID_ARABICDARIJAENTRY = ARTE.ID_ARABICDARIJAENTRY AND TextEntity_Type = 'MAIN ENTITY' "
                    + "ORDER BY ARZ.ArabiziEntryDate DESC ";

                conn.Open();
                return conn.Query<ArabiziToArabicViewModel2>(qry).ToList();
            }
        }*/

        private List<ArabiziToArabicViewModel> loadArabiziToArabicViewModel_DAPPERSQL(bool activeThemeOnly)
        {
            if (activeThemeOnly)
            {
                String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    String qry = "SELECT "
                            + "ARZ.ID_ARABIZIENTRY, "
                            + "ARZ.ArabiziEntryDate, "
                            + "ARZ.ArabiziText, "
                            + "AR.ID_ARABICDARIJAENTRY, "
                            + "AR.ArabicDarijaText, "
                            + "ARTE.TextEntity_Mention "
                        + "FROM T_ARABIZIENTRY ARZ "
                        + "INNER JOIN T_ARABICDARIJAENTRY AR ON ARZ.ID_ARABIZIENTRY = AR.ID_ARABIZIENTRY "
                        + "INNER JOIN T_ARABICDARIJAENTRY_TEXTENTITY ARTE ON AR.ID_ARABICDARIJAENTRY = ARTE.ID_ARABICDARIJAENTRY AND TextEntity_Type = 'MAIN ENTITY' "
                        + "INNER JOIN T_XTRCTTHEME XT ON XT.ThemeName = ARTE.TextEntity_Mention AND XT.CurrentActive = 'active' "
                        + "ORDER BY ARZ.ArabiziEntryDate DESC ";

                    conn.Open();
                    return conn.Query<ArabiziToArabicViewModel>(qry).ToList();
                }
            }
            else
                return loadArabiziToArabicViewModel_DAPPERSQL();
        }

        private int loadArabiziToArabicViewModelCount_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT COUNT(*) "
                    + "FROM T_ARABIZIENTRY ";

                conn.Open();
                return conn.QueryFirst<int>(qry);
            }
        }
        #endregion

        #region BACK YARD BO SAVE / DELETE
        private void saveserializeM_ARABIZIENTRY(M_ARABIZIENTRY arabiziEntry, AccessMode accessMode)
        {
            if (accessMode == AccessMode.xml)
                saveserializeM_ARABIZIENTRY_XML(arabiziEntry);
            else if (accessMode == AccessMode.efsql)
            {
                saveserializeM_ARABIZIENTRY_EFSQL(arabiziEntry);
            }
        }

        private void saveserializeM_ARABIZIENTRY_XML(M_ARABIZIENTRY arabiziEntry)
        {
            // save
            String path = Server.MapPath("~/App_Data/data_M_ARABIZIENTRY.txt");
            new TextPersist().Serialize(arabiziEntry, path);
        }

        private void saveserializeM_ARABIZIENTRY_EFSQL(M_ARABIZIENTRY arabiziEntry)
        {
            using (var db = new ArabiziDbContext())
            {
                db.M_ARABIZIENTRYs.Add(arabiziEntry);

                // commit
                db.SaveChanges();
            }
        }

        private void saveserializeM_ARABICDARIJAENTRY(M_ARABICDARIJAENTRY arabicDarijaEntry, AccessMode accessMode)
        {
            if (accessMode == AccessMode.xml)
                saveserializeM_ARABICDARIJAENTRY_XML(arabicDarijaEntry);
            else if (accessMode == AccessMode.efsql)
            {
                saveserializeM_ARABICDARIJAENTRY_EFSQL(arabicDarijaEntry);
            }
        }

        private void saveserializeM_ARABICDARIJAENTRY_EFSQL(M_ARABICDARIJAENTRY arabicDarijaEntry)
        {
            using (var db = new ArabiziDbContext())
            {
                db.M_ARABICDARIJAENTRYs.Add(arabicDarijaEntry);

                // commit
                db.SaveChanges();
            }
        }

        private void saveserializeM_ARABICDARIJAENTRY_XML(M_ARABICDARIJAENTRY arabicDarijaEntry)
        {
            String path = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY.txt");
            new TextPersist().Serialize(arabicDarijaEntry, path);
        }

        private void saveserializeM_ARABICDARIJAENTRY_LATINWORD(M_ARABICDARIJAENTRY_LATINWORD latinWord, AccessMode accessMode)
        {
            if (accessMode == AccessMode.xml)
                saveserializeM_ARABICDARIJAENTRY_LATINWORD_XML(latinWord);
            else if (accessMode == AccessMode.efsql)
                saveserializeM_ARABICDARIJAENTRY_LATINWORD_EFSQL(latinWord);
        }

        private void saveserializeM_ARABICDARIJAENTRY_LATINWORD_EFSQL(M_ARABICDARIJAENTRY_LATINWORD latinWord)
        {
            using (var db = new ArabiziDbContext())
            {
                db.M_ARABICDARIJAENTRY_LATINWORDs.Add(latinWord);

                // commit
                db.SaveChanges();
            }
        }

        private void saveserializeM_ARABICDARIJAENTRY_LATINWORD_XML(M_ARABICDARIJAENTRY_LATINWORD latinWord)
        {
            // Save to Serialization
            string path = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY_LATINWORD.txt");
            new TextPersist().Serialize(latinWord, path);
        }

        private void saveserializeM_ARABICDARIJAENTRY_LATINWORD_XML(TextSentiment sentiment)
        {
            string path = Server.MapPath("~/App_Data/data_TextSentiment.txt");
            new TextPersist().Serialize(sentiment, path);
        }

        private void saveserializeM_ARABICDARIJAENTRY_TEXTENTITY(M_ARABICDARIJAENTRY_TEXTENTITY textEntity, AccessMode accessMode)
        {
            if (accessMode == AccessMode.xml)
                saveserializeM_ARABICDARIJAENTRY_TEXTENTITY_XML(textEntity);
            else if (accessMode == AccessMode.efsql)
                saveserializeM_ARABICDARIJAENTRY_TEXTENTITY_EFSQL(textEntity);
        }

        private void saveserializeM_ARABICDARIJAENTRY_TEXTENTITY_EFSQL(M_ARABICDARIJAENTRY_TEXTENTITY textEntity)
        {
            using (var db = new ArabiziDbContext())
            {
                db.M_ARABICDARIJAENTRY_TEXTENTITYs.Add(textEntity);

                // commit
                db.SaveChanges();
            }
        }

        private void serialize_Delete_M_ARABICDARIJAENTRY_TEXTENTITY_EFSQL(M_ARABICDARIJAENTRY_TEXTENTITY textEntity)
        {
            using (var db = new ArabiziDbContext())
            {
                // Note: first attach to the entity
                db.M_ARABICDARIJAENTRY_TEXTENTITYs.Attach(textEntity);

                // remove
                db.M_ARABICDARIJAENTRY_TEXTENTITYs.Remove(textEntity);

                // commit
                db.SaveChanges();
            }
        }

        private void serialize_Delete_M_ARABICDARIJAENTRY_TEXTENTITY_EFSQL(List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities)
        {
            if (textEntities.Count > 0)
            {
                using (var db = new ArabiziDbContext())
                {
                    // Note: first attach to the entity
                    foreach (var textEntity in textEntities)
                        db.M_ARABICDARIJAENTRY_TEXTENTITYs.Attach(textEntity);

                    // remove
                    db.M_ARABICDARIJAENTRY_TEXTENTITYs.RemoveRange(textEntities);

                    // commit
                    db.SaveChanges();
                }
            }
        }

        private void saveserializeM_ARABICDARIJAENTRY_TEXTENTITY_XML(M_ARABICDARIJAENTRY_TEXTENTITY textEntity)
        {
            // Save to Serialization
            var path = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY_TEXTENTITY.txt");
            new TextPersist().Serialize(textEntity, path);
        }

        private void saveserializeM_XTRCTTHEME_EFSQL(M_XTRCTTHEME m_xtrcttheme)
        {
            using (var db = new ArabiziDbContext())
            {
                db.M_XTRCTTHEMEs.Add(m_xtrcttheme);

                // commit
                db.SaveChanges();
            }
        }

        public void saveserializeM_XTRCTTHEME_KEYWORDs_EFSQL(List<M_XTRCTTHEME_KEYWORD> m_xtrcttheme_keywords)
        {
            using (var db = new ArabiziDbContext())
            {
                //
                db.Database.ExecuteSqlCommand("DELETE FROM T_XTRCTTHEME_KEYWORD");
                db.M_XTRCTTHEME_KEYWORDs.AddRange(m_xtrcttheme_keywords);

                // commit
                db.SaveChanges();
            }
        }

        public void saveserializeM_XTRCTTHEME_KEYWORDs_DELETE_ALL_EFSQL(List<M_XTRCTTHEME_KEYWORD> m_xtrcttheme_keywords, M_XTRCTTHEME currentTheme)
        {
            using (var db = new ArabiziDbContext())
            {
                //
                db.Database.ExecuteSqlCommand("DELETE FROM T_XTRCTTHEME_KEYWORD WHERE ID_XTRCTTHEME = '" + currentTheme.ID_XTRCTTHEME + "' ");

                // commit
                db.SaveChanges();
            }
        }

        public void saveserializeM_XTRCTTHEME_KEYWORDs_EFSQL(List<M_XTRCTTHEME_KEYWORD> m_xtrcttheme_keywords, M_XTRCTTHEME currentTheme)
        {
            using (var db = new ArabiziDbContext())
            {
                //
                db.Database.ExecuteSqlCommand("DELETE FROM T_XTRCTTHEME_KEYWORD WHERE ID_XTRCTTHEME = '" + currentTheme.ID_XTRCTTHEME + "' ");

                // commit
                db.SaveChanges();

                // reload
                db.M_XTRCTTHEME_KEYWORDs.AddRange(m_xtrcttheme_keywords);

                // commit
                db.SaveChanges();
            }
        }

        private void saveserializeM_XTRCTTHEME_EFSQL_Active(String themename)
        {
            using (var db = new ArabiziDbContext())
            {
                var xtrctThemes = db.M_XTRCTTHEMEs;

                var tobeactiveXtrctTheme = xtrctThemes.Where(m => m.ThemeName == themename).FirstOrDefault<M_XTRCTTHEME>();
                tobeactiveXtrctTheme.CurrentActive = "active";

                // commit
                db.SaveChanges();
            }
        }

        private void saveserializeM_XTRCTTHEME_EFSQL_Deactivate()
        {
            using (var db = new ArabiziDbContext())
            {
                var xtrctThemes = db.M_XTRCTTHEMEs;

                var tobeactiveXtrctTheme = xtrctThemes.Where(m => m.CurrentActive == "active").FirstOrDefault<M_XTRCTTHEME>();
                tobeactiveXtrctTheme.CurrentActive = String.Empty;

                // commit
                db.SaveChanges();
            }
        }

        public void Serialize_Delete_M_ARABIZIENTRY_Cascading_EFSQL(Guid id_arabizientry)
        {
            using (var db = new ArabiziDbContext())
            {
                // filter on the one linked to current arabizi entry
                var arabicdarijaentry = db.M_ARABICDARIJAENTRYs.Single(m => m.ID_ARABIZIENTRY == id_arabizientry);

                // load/deserialize data_M_ARABICDARIJAENTRY_TEXTENTITY
                // filter on the ones linked to current arabic darija entry
                db.M_ARABICDARIJAENTRY_TEXTENTITYs.RemoveRange(db.M_ARABICDARIJAENTRY_TEXTENTITYs.Where(m => m.ID_ARABICDARIJAENTRY == arabicdarijaentry.ID_ARABICDARIJAENTRY));

                // load/deserialize M_ARABICDARIJAENTRY_LATINWORD
                // filter on the ones linked to current arabic darija entry
                db.M_ARABICDARIJAENTRY_LATINWORDs.RemoveRange(db.M_ARABICDARIJAENTRY_LATINWORDs.Where(m => m.ID_ARABICDARIJAENTRY == arabicdarijaentry.ID_ARABICDARIJAENTRY));

                // remove arabic darija item
                db.M_ARABICDARIJAENTRYs.Remove(arabicdarijaentry);

                // remove arabizi
                db.M_ARABIZIENTRYs.Remove(db.M_ARABIZIENTRYs.Single(m => m.ID_ARABIZIENTRY == id_arabizientry));

                // commit
                db.SaveChanges();
            }
        }
        #endregion

        #region BACK YARD BO HELPERS
        /*public static IEnumerable<TSource> DistinctBy<TSource, TKey>
    (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }*/

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        #endregion
    }

    //Class for call the APIs by html
    public static class HtmlHelpers
    {
        public static async Task<string> PostAPIRequest(string url, string para, string type = "POST")
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
                if (!string.IsNullOrEmpty(type) && type == "POST")
                {
                    var response = await client.PostAsync(url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        dynamic dynamicObject = JObject.Parse(result);
                        if (dynamicObject.status != null)
                        {
                            result = Convert.ToString(dynamicObject.status);
                        }
                    }
                    else
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                }
                if (!string.IsNullOrEmpty(type) && type == "GET")
                {
                    try
                    {
                        var response = await new HttpClient().GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            result = await response.Content.ReadAsStringAsync();
                            //var dynamicObject = JsonConvert.DeserializeObject(result);
                            dynamic dynamicObject = JObject.Parse(result);
                            if (dynamicObject.M_ARABICDARIJAENTRY != null)
                            {
                                result = "Success" + Convert.ToString(dynamicObject.M_ARABICDARIJAENTRY.ArabicDarijaText);
                            }

                            //Assert.IsTrue(("تجريبى" == trad) || ("تجريبة" == trad));
                        }
                        else
                        {
                            result = await response.Content.ReadAsStringAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        result = e.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        public static async Task<Tuple<String, String>> PostAPIRequest_message(string url, string para, string type = "POST")
        {
            HttpClient client;
            string result = string.Empty;
            String message = String.Empty;

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
                if (!string.IsNullOrEmpty(type) && type == "POST")
                {
                    var response = await client.PostAsync(url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        result = await response.Content.ReadAsStringAsync();
                        dynamic dynamicObject = JObject.Parse(result);
                        if (dynamicObject.status != null)
                        {
                            result = Convert.ToString(dynamicObject.status);
                            message = Convert.ToString(dynamicObject.message);
                        }
                    }
                    else
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return new Tuple<String, String>(result, message);
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
                    result = ex.Message;

                }

            }
            return result;


        }
    }
}
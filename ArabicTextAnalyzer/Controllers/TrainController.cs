using ArabicTextAnalyzer.Business.Provider;
using ArabicTextAnalyzer.Domain;
using ArabicTextAnalyzer.Domain.Models;
using ArabicTextAnalyzer.Models;
using ArabicTextAnalyzer.ViewModels;
using Dapper;
using OADRJNLPCommon.Business;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace ArabicTextAnalyzer.Controllers
{
    public class TrainController : Controller
    {
        private static Object thisLock = new Object();

        // GET: Train
        public ActionResult Index()
        {
            String dataPath = Server.MapPath("~/App_Data");

            // send size of corpus & co-data
            @ViewBag.CorpusSize = new TextFrequency().GetCorpusNumberOfLine();
            @ViewBag.CorpusWordCount = new TextFrequency().GetCorpusWordCount();
            @ViewBag.BidictSize = new TextFrequency().GetBidictNumberOfLine();
            @ViewBag.ArabiziEntriesCount = new TextFrequency().GetArabiziEntriesCount(dataPath);
            @ViewBag.RatioLatinWordsOnEntries = new TextFrequency().GetRatioLatinWordsOnEntries(dataPath);
            @ViewBag.EntitiesCount = new TextFrequency().GetEntitiesCount();

            // deserialize/send twingly accounts
            @ViewBag.TwinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(dataPath);

            // themes : deserialize/send list of themes, plus send active theme, plus send list of tags/keywords
            var xtrctThemes = new TextPersist().Deserialize<M_XTRCTTHEME>(dataPath);
            var xtrctThemesKeywords = new TextPersist().Deserialize<M_XTRCTTHEME_KEYWORD>(dataPath);
            var activeXtrctTheme = xtrctThemes.Find(m => m.CurrentActive == "active");
            @ViewBag.XtrctThemes = xtrctThemes;
            @ViewBag.XtrctThemesPlain = xtrctThemes.Select(m => new SelectListItem { Text = m.ThemeName });
            @ViewBag.ActiveXtrctTheme = activeXtrctTheme;
            @ViewBag.ActiveXtrctThemeTags = xtrctThemesKeywords.Where(m => m.ID_XTRCTTHEME == activeXtrctTheme.ID_XTRCTTHEME).ToList();

            // file upload communication
            @ViewBag.showAlertWarning = TempData["showAlertWarning"] != null ? TempData["showAlertWarning"] : false;
            @ViewBag.showAlertSuccess = TempData["showAlertSuccess"] != null ? TempData["showAlertSuccess"] : false;
            @ViewBag.msgAlert = TempData["msgAlert"] != null ? TempData["msgAlert"] : String.Empty;
            TempData.Remove("showAlertWarning");
            TempData.Remove("showAlertSuccess");
            TempData.Remove("msgAlert");

            //
            return View();
        }

        [HttpPost]
        public ActionResult ArabicDarijaEntryPartialView()
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                // This action is called at each reload of train main view, via Ajax to fill the partial view of the grid arabizi/arabic

                //
                var accessMode = AccessMode.dappersql;
                // var accessMode = AccessMode.xml;

                // load/deserialize M_ARABICDARIJAENTRY
                List<M_ARABICDARIJAENTRY> entries = loaddeserializeM_ARABICDARIJAENTRY(accessMode);

                // load/deserialize M_ARABICDARIJAENTRY_LATINWORD
                List<M_ARABICDARIJAENTRY_LATINWORD> latinWordsEntries = loaddeserializeM_ARABICDARIJAENTRY_LATINWORD(accessMode);

                // load/deserialize M_ARABIZIENTRY
                List<M_ARABIZIENTRY> arabiziEntries = loaddeserializeM_ARABIZIENTRY(accessMode);

                // load/deserialize list of M_ARABICDARIJAENTRY_TEXTENTITY
                List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = loaddeserializeM_ARABICDARIJAENTRY_TEXTENTITY(accessMode);

                watch.Stop();
                var elapsedMs0 = watch.ElapsedMilliseconds;
                watch.Start();

                //
                List<Class2> xs = new List<Class2>();
                foreach (M_ARABICDARIJAENTRY arabicdarijaentry in entries)
                {
                    var perEntryLatinWordsEntries = latinWordsEntries.Where(m => m.ID_ARABICDARIJAENTRY == arabicdarijaentry.ID_ARABICDARIJAENTRY).ToList();
                    var perEntryTextEntities = textEntities.Where(m => m.ID_ARABICDARIJAENTRY == arabicdarijaentry.ID_ARABICDARIJAENTRY).ToList();
                    var x = new Class2
                    {
                        ArabicDarijaEntry = arabicdarijaentry,
                        ArabicDarijaEntryLatinWords = perEntryLatinWordsEntries,
                        ArabiziEntry = arabiziEntries.Single(m => m.ID_ARABIZIENTRY == arabicdarijaentry.ID_ARABIZIENTRY),
                        TextEntities = perEntryTextEntities
                    };
                    xs.Add(x);
                }

                // reverse order to latest entry in top
                    xs.Reverse();

                // themes / main entities : send list of main tags
                var dataPath = Server.MapPath("~/App_Data/");
                var xtrctThemes = new TextPersist().Deserialize<M_XTRCTTHEME>(dataPath);

                // 
                var class1 = new Class1
                {
                    Classes2 = xs.Take(100).ToList(),
                    MainEntities = xtrctThemes
                };

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;

                // pass entries to partial view via the model (instead of the bag for a view)
                return PartialView("_IndexPartialPage_arabicDarijaEntries", class1);
            }
            catch (Exception ex)
            {
                Logging.Write(Server, ex.Message);
                Logging.Write(Server, ex.StackTrace);

                return null;
            }
        }

        [HttpPost]
        public ActionResult TrainStepOne(M_ARABIZIENTRY arabiziEntry)
        {
            // Arabizi to arabic script via direct call to perl script
            train(arabiziEntry);

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
            List<M_TWINGLYACCOUNT> twinglyAccounts = new List<M_TWINGLYACCOUNT>();
            var path = Server.MapPath("~/App_Data/data_" + typeof(M_TWINGLYACCOUNT).Name + ".txt");
            var serializer = new XmlSerializer(twinglyAccounts.GetType());
            using (var reader = new System.IO.StreamReader(path))
            {
                twinglyAccounts = (List<M_TWINGLYACCOUNT>)serializer.Deserialize(reader);
            }
            String twinglyApiKey = twinglyAccounts.Find(m => m.CurrentActive == "active").ID_TWINGLYACCOUNT_API_KEY.ToString();

            // deserialize M_ARABICDARIJAENTRY_LATINWORD to check if most popular already found
            List<M_ARABICDARIJAENTRY_LATINWORD> latinWordsEntries = new List<M_ARABICDARIJAENTRY_LATINWORD>();
            path = Server.MapPath("~/App_Data/data_" + typeof(M_ARABICDARIJAENTRY_LATINWORD).Name + ".txt");
            serializer = new XmlSerializer(latinWordsEntries.GetType());
            using (var reader = new System.IO.StreamReader(path))
            {
                latinWordsEntries = (List<M_ARABICDARIJAENTRY_LATINWORD>)serializer.Deserialize(reader);
            }
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
                    /*var calls_free = OADRJNLPCommon.Business.Business.getTwinglyAccountInfo_calls_free(twinglyApi15Url, twinglyApiKey);
                    path = Server.MapPath("~/App_Data/data_M_TWINGLYACCOUN.txt");
                    new TextPersist().Serialize_Update_M_TWINGLYACCOUNT_calls_free(new Guid(twinglyApiKey), calls_free, path);*/
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
                        path = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY_LATINWORD.txt");
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
            var dataPath = Server.MapPath("~/App_Data/");
            new TextPersist().Serialize_Delete_M_ARABIZIENTRY_Cascading(arabiziWordGuid, dataPath);

            //
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Train_DeleteEntries(String arabiziWordGuids)
        {
            var larabiziWordGuids = arabiziWordGuids.Split(new char[] { ',' });

            var dataPath = Server.MapPath("~/App_Data/");

            foreach (var larabiziWordGuid in larabiziWordGuids)
            {
                Guid arabiziWordGuid = new Guid(larabiziWordGuid);
                new TextPersist().Serialize_Delete_M_ARABIZIENTRY_Cascading(arabiziWordGuid, dataPath);
            }

            //
            return RedirectToAction("Index");
        }

        // This action applies a new main tag/entity/theme/keyword to a post
        [HttpGet]
        public ActionResult Train_ApplyNewMainTag(Guid idArabicDarijaEntry, String mainEntity)
        {
            // load M_ARABICDARIJAENTRY_TEXTENTITY
            var dataPath = Server.MapPath("~/App_Data");
            var arabicDarijaEntryTextEntities = new TextPersist().Deserialize<M_ARABICDARIJAENTRY_TEXTENTITY>(dataPath);

            // Check before if already main entity
            if (arabicDarijaEntryTextEntities.Find(m => m.ID_ARABICDARIJAENTRY == idArabicDarijaEntry && m.TextEntity.Mention == mainEntity && m.TextEntity.Type == "MAIN ENTITY") != null)
            {
                TempData["showAlertWarning"] = true;
                TempData["msgAlert"] = "'" + mainEntity + "' is already MAIN ENTITY for the post";
                return RedirectToAction("Index");
            }

            // apply main tag
            arabicDarijaEntryTextEntities.Add(new M_ARABICDARIJAENTRY_TEXTENTITY
            {
                ID_ARABICDARIJAENTRY_TEXTENTITY = Guid.NewGuid(),
                ID_ARABICDARIJAENTRY = idArabicDarijaEntry,
                TextEntity = new TextEntity
                {
                    Count = 1,
                    Mention = mainEntity,
                    Type = "MAIN ENTITY"
                }
            });
            new TextPersist().SerializeBack_dataPath(arabicDarijaEntryTextEntities, dataPath);

            //
            TempData["showAlertSuccess"] = true;
            TempData["msgAlert"] = "'" + mainEntity + "' is now MAIN ENTITY for the post";
            return RedirectToAction("Index");
        }

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
            var twinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));

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
            var twinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));

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
            var twinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));

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
                ThemeName = themename
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
            String dataPath = Server.MapPath("~/App_Data");
            var xtrctThemes = new TextPersist().Deserialize<M_XTRCTTHEME>(dataPath);
            var activeXtrctTheme = xtrctThemes.Find(m => m.CurrentActive == "active");
            activeXtrctTheme.CurrentActive = String.Empty;

            // find to-be-active by name, and make it active
            var tobeactiveXtrctTheme = xtrctThemes.Find(m => m.ThemeName == themename);
            tobeactiveXtrctTheme.CurrentActive = "active";

            // save
            new TextPersist().SerializeBack_dataPath(xtrctThemes, dataPath);

            //
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult XtrctTheme_Keywords_Reload(String themename)
        {
            String dataPath = Server.MapPath("~/App_Data");

            // we look for the ners that are associated with each entry for the current theme 
            var textEntities = new List<M_ARABICDARIJAENTRY_TEXTENTITY>();
            var arabicDarijaEntryTextEntities = new TextPersist().Deserialize<M_ARABICDARIJAENTRY_TEXTENTITY>(dataPath);
            var arabicDarijaEntryTextMainEntities = arabicDarijaEntryTextEntities.FindAll(m => m.TextEntity.Type == "MAIN ENTITY" && m.TextEntity.Mention == themename);
            foreach (var mainentity in arabicDarijaEntryTextMainEntities)
            {
                textEntities.AddRange(arabicDarijaEntryTextEntities.FindAll(m => m.ID_ARABICDARIJAENTRY == mainentity.ID_ARABICDARIJAENTRY));
            }

            // if not existing, add them to list of theme keywords
            var xtrctThemes = new TextPersist().Deserialize<M_XTRCTTHEME>(dataPath);
            var xtrctThemesKeywords = new TextPersist().Deserialize<M_XTRCTTHEME_KEYWORD>(dataPath);
            var activeXtrctTheme = xtrctThemes.Find(m => m.CurrentActive == "active");
            foreach (var entity in textEntities)
            {
                if (xtrctThemesKeywords.Find(m => m.ID_XTRCTTHEME == activeXtrctTheme.ID_XTRCTTHEME && m.Keyword == entity.TextEntity.Mention) == null)
                {
                    xtrctThemesKeywords.Add(new M_XTRCTTHEME_KEYWORD
                    {
                        ID_XTRCTTHEME_KEYWORD = Guid.NewGuid(),
                        ID_XTRCTTHEME = activeXtrctTheme.ID_XTRCTTHEME,
                        Keyword = entity.TextEntity.Mention
                    });
                }
            }

            // save
            new TextPersist().SerializeBack_dataPath(xtrctThemesKeywords, dataPath);

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

            Logging.Write(Server, "Data_Upload - 3");

            // extract only the filename
            var fileName = Path.GetFileName(file.FileName);

            Logging.Write(Server, "Data_Upload - 4");

            // store the file inside ~/App_Data/uploads folder
            var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
            file.SaveAs(path);

            Logging.Write(Server, "Data_Upload - 5");

            // loop and process each one
            var lines = System.IO.File.ReadLines(path).ToList();
            foreach (string line in lines)
            {
                var idArabicDarijaEntry = train(new M_ARABIZIENTRY
                {
                    ArabiziText = line.Trim(),
                    ArabiziEntryDate = DateTime.Now
                });

                // add main entity & Save to Serialization
                var textEntity = new M_ARABICDARIJAENTRY_TEXTENTITY
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
                var pathtextentity = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY_TEXTENTITY.txt");
                new TextPersist().Serialize(textEntity, pathtextentity);
            }

            // mark how many rows been translated
            TempData["showAlertSuccess"] = true;
            TempData["msgAlert"] = lines.Count.ToString() + " rows has been imported.";

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

        #region BACK YARD BO TRAIN
        private Guid train(M_ARABIZIENTRY arabiziEntry)
        {
            // Arabizi to arabic from perl script
            if (arabiziEntry.ArabiziText == null)
                return Guid.Empty;

            //
            var id_ARABICDARIJAENTRY = Guid.NewGuid();

            Logging.Write(Server, "train - before lock");
            lock (thisLock)
            {
                String arabicText = train_savearabizi(arabiziEntry);

                arabicText = train_bidict(arabicText);

                arabicText = train_binggoogle(arabicText);

                arabicText = train_perl(arabicText, arabiziEntry.ID_ARABIZIENTRY, id_ARABICDARIJAENTRY);

                arabicText = train_latinwords(arabicText, id_ARABICDARIJAENTRY);

                train_sa(arabicText);

                train_ner(arabicText, id_ARABICDARIJAENTRY);
            }
            Logging.Write(Server, "train - after lock");

            //
            return id_ARABICDARIJAENTRY;
        }

        private String train_savearabizi(M_ARABIZIENTRY arabiziEntry)
        {
            // complete arabizi entry & Save arabiziEntry to Serialization
            arabiziEntry.ID_ARABIZIENTRY = Guid.NewGuid();

            // clean
            arabiziEntry.ArabiziText = arabiziEntry.ArabiziText.Trim(new char[] { ' ', '\t' });

            // save
            String path = Server.MapPath("~/App_Data/data_M_ARABIZIENTRY.txt");
            new TextPersist().Serialize(arabiziEntry, path);

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

        private String train_perl(string arabicText, Guid id_ARABIZIENTRY, Guid id_ARABICDARIJAENTRY)
        {
            var regex = new Regex(@"<span class='notranslate'>(.*?)</span>");

            // save matches
            var matches = regex.Matches(arabicText);

            // skip over non-translantable parts
            arabicText = regex.Replace(arabicText, "001000100");

            // translate arabiza to darija arabic script using perl script via direct call and save arabicDarijaEntry to Serialization
            arabicText = new TextConverter().Convert(arabicText);

            // restore do not translate
            var regex2 = new Regex(@"001000100");
            foreach (Match match in matches)
            {
                arabicText = regex2.Replace(arabicText, match.Value, 1);
            }
            // clean <span class='notranslate'>
            arabicText = new Regex(@"<span class='notranslate'>(.*?)</span>").Replace(arabicText, "$1");

            // save
            var arabicDarijaEntry = new M_ARABICDARIJAENTRY
            {
                ID_ARABICDARIJAENTRY = id_ARABICDARIJAENTRY,
                ID_ARABIZIENTRY = id_ARABIZIENTRY,
                ArabicDarijaText = arabicText
            };
            String path = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY.txt");
            new TextPersist().Serialize(arabicDarijaEntry, path);

            //
            return arabicText;
        }

        private String train_latinwords(String arabicText, Guid id_ARABICDARIJAENTRY)
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
                string path = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY_LATINWORD.txt");
                new TextPersist().Serialize(latinWord, path);
            }

            return arabicText;
        }

        private void train_sa(string arabicText)
        {
            // Sentiment analysis from watson https://gateway.watsonplatform.net/" and Save to Serialization
            var textSentimentAnalyzer = new TextSentimentAnalyzer();
            var sentiment = textSentimentAnalyzer.GetSentiment(arabicText);
            string path = Server.MapPath("~/App_Data/data_TextSentiment.txt");
            new TextPersist().Serialize(sentiment, path);
        }

        private void train_ner(string arabicText, Guid id_ARABICDARIJAENTRY)
        {
            // Entity extraction from rosette (https://api.rosette.com/rest/v1/)
            var entities = new TextEntityExtraction().GetEntities(arabicText);

            // NER manual extraction
            new TextEntityExtraction().NerManualExtraction(arabicText, entities, id_ARABICDARIJAENTRY, Server);
        }
        #endregion

        #region BACK YARD BO LOAD
        enum AccessMode
        {
            dappersql,
            efsql,
            xml
        }

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
                String qry = "SELECT * FROM T_ARABICDARIJAENTRY";

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
}
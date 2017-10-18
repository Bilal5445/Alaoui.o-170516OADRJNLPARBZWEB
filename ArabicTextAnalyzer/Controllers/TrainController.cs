using ArabicTextAnalyzer.Business.Provider;
using ArabicTextAnalyzer.Domain;
using ArabicTextAnalyzer.Domain.Models;
using ArabicTextAnalyzer.Models;
using ArabicTextAnalyzer.ViewModels;
using OADRJNLPCommon.Business;
using System;
using System.Collections.Generic;
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
            // This action is called at each reload of train main view, via Ajax to fill the partial view of the grid arabizi/arabic

            // load/deserialize M_ARABICDARIJAENTRY
            List<M_ARABICDARIJAENTRY> entries = new List<M_ARABICDARIJAENTRY>();
            String path = Server.MapPath("~/App_Data/data_" + typeof(M_ARABICDARIJAENTRY).Name + ".txt");
            XmlSerializer serializer = new XmlSerializer(entries.GetType());
            using (var reader = new System.IO.StreamReader(path))
            {
                entries = (List<M_ARABICDARIJAENTRY>)serializer.Deserialize(reader);
            }

            // load/deserialize M_ARABICDARIJAENTRY_LATINWORD
            List<M_ARABICDARIJAENTRY_LATINWORD> latinWordsEntries = new List<M_ARABICDARIJAENTRY_LATINWORD>();
            path = Server.MapPath("~/App_Data/data_" + typeof(M_ARABICDARIJAENTRY_LATINWORD).Name + ".txt");
            serializer = new XmlSerializer(latinWordsEntries.GetType());
            using (var reader = new System.IO.StreamReader(path))
            {
                latinWordsEntries = (List<M_ARABICDARIJAENTRY_LATINWORD>)serializer.Deserialize(reader);
            }

            // load/deserialize M_ARABIZIENTRY
            List<M_ARABIZIENTRY> arabiziEntries = new List<M_ARABIZIENTRY>();
            path = Server.MapPath("~/App_Data/data_" + typeof(M_ARABIZIENTRY).Name + ".txt");
            serializer = new XmlSerializer(arabiziEntries.GetType());
            using (var reader = new System.IO.StreamReader(path))
            {
                arabiziEntries = (List<M_ARABIZIENTRY>)serializer.Deserialize(reader);
            }

            // load/deserialize list of M_ARABICDARIJAENTRY_TEXTENTITY
            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = new List<M_ARABICDARIJAENTRY_TEXTENTITY>();
            path = Server.MapPath("~/App_Data/data_" + typeof(M_ARABICDARIJAENTRY_TEXTENTITY).Name + ".txt");
            serializer = new XmlSerializer(textEntities.GetType());
            if (System.IO.File.Exists(path))
            {
                using (var reader = new System.IO.StreamReader(path))
                {
                    textEntities = (List<M_ARABICDARIJAENTRY_TEXTENTITY>)serializer.Deserialize(reader);
                }
            }

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
            /*var mainEntities = textEntities.Where(m => m.TextEntity.Type == "MAIN ENTITY");
            mainEntities = DistinctBy(mainEntities, m => m.TextEntity.Mention);*/
            var dataPath = Server.MapPath("~/App_Data/");
            var xtrctThemes = new TextPersist().Deserialize<M_XTRCTTHEME>(dataPath);

            // 
            var class1 = new Class1
            {
                Classes2 = xs,
                // MainEntities = mainEntities
                MainEntities = xtrctThemes
            };

            // pass entries to partial view via the model (instead of the bag for a view)
            return PartialView("_IndexPartialPage_arabicDarijaEntries", class1);
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

        #region BACK YARD ACTIONS TWINGLY
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
        #endregion

        #region BACK YARD ACTIONS THEME
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
                    ArabiziText = line,
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

        #region BACK YARD BO
        private Guid train(M_ARABIZIENTRY arabiziEntry)
        {
            Logging.Write(Server, "train - 1");

            // Arabizi to arabic script via direct call to perl script
            var textConverter = new TextConverter();

            Logging.Write(Server, "train - 2");

            // Arabizi to arabic from perl script
            if (arabiziEntry.ArabiziText != null)
            {
                var id_ARABICDARIJAENTRY = Guid.NewGuid();

                Logging.Write(Server, "train - 3 - before lock");

                lock (thisLock)
                {
                    // complete arabizi entry
                    arabiziEntry.ID_ARABIZIENTRY = Guid.NewGuid();

                    // first pass : correct/translate the original arabizi into msa arabic using big/google apis (to take care of french/english segments in codeswitch arabizi posts)
                    var arabiziTextToMsaFirstPass = new TranslationTools().CorrectTranslate(arabiziEntry.ArabiziText/*, Server*/);

                    // prepare darija from perl script
                    var arabicText = textConverter.Convert(/*arabiziEntry.ArabiziText*/arabiziTextToMsaFirstPass);
                    var arabicDarijaEntry = new M_ARABICDARIJAENTRY
                    {
                        // ID_ARABICDARIJAENTRY = Guid.NewGuid(),
                        ID_ARABICDARIJAENTRY = id_ARABICDARIJAENTRY,
                        ID_ARABIZIENTRY = arabiziEntry.ID_ARABIZIENTRY,
                        ArabicDarijaText = arabicText
                    };

                    Logging.Write(Server, "train - 4");

                    // Save arabiziEntry to Serialization
                    String path = Server.MapPath("~/App_Data/data_M_ARABIZIENTRY.txt");
                    new TextPersist().Serialize(arabiziEntry, path);

                    Logging.Write(Server, "train - 5");

                    // Save arabicDarijaEntry to Serialization
                    path = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY.txt");
                    new TextPersist().Serialize(arabicDarijaEntry, path);

                    Logging.Write(Server, "train - 6");

                    // latin words
                    MatchCollection matches = TextTools.ExtractLatinWords(arabicDarijaEntry.ArabicDarijaText);

                    // save every match of latin words
                    // also calculate on the fly the number of variants
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

                        // See if we can further correct/translate any latin words
                        var translatedLatinWord = new TranslationTools().CorrectTranslate(arabiziWord/*, Server*/);
                        latinWord.Translation = translatedLatinWord;

                        // if any replacein arabic text (TODO : use match to replace the match and not search/replace to better handle duplicate)
                        arabicText = arabicText.Replace(match.Value, translatedLatinWord);

                        // Save to Serialization
                        path = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY_LATINWORD.txt");
                        new TextPersist().Serialize(latinWord, path);
                    }

                    Logging.Write(Server, "train - 7");

                    // Sentiment analysis from watson https://gateway.watsonplatform.net/";
                    var textSentimentAnalyzer = new TextSentimentAnalyzer();
                    var sentiment = textSentimentAnalyzer.GetSentiment(arabicText);
                    // Save to Serialization
                    path = Server.MapPath("~/App_Data/data_TextSentiment.txt");
                    new TextPersist().Serialize(sentiment, path);

                    Logging.Write(Server, "train - 8");

                    // Entity extraction from rosette (https://api.rosette.com/rest/v1/)
                    var textEntityExtraction = new TextEntityExtraction();
                    var entities = textEntityExtraction.GetEntities(arabicText);

                    Logging.Write(Server, "train - 9");

                    // NER manual extraction
                    new TextEntityExtraction().NerManualExtraction(arabicText, entities, arabicDarijaEntry.ID_ARABICDARIJAENTRY, Server);
                }
                Logging.Write(Server, "train - 10 - after lock");

                //
                return id_ARABICDARIJAENTRY;
            }

            return Guid.Empty;
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
using ArabicTextAnalyzer.Business.Provider;
using ArabicTextAnalyzer.Domain.Models;
using ArabicTextAnalyzer.Models;
using ArabicTextAnalyzer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace ArabicTextAnalyzer.Controllers
{
    public class TrainController : Controller
    {
        // GET: Train
        public ActionResult Index()
        {
            // send size of corpus
            @ViewBag.CorpusSize = new TextFrequency().GetCorpusNumberOfLine();
            @ViewBag.CorpusWordCount = new TextFrequency().GetCorpusWordCount();

            // deserialize & send twingly accounts
            @ViewBag.TwinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));

            //
            return View();
        }

        [HttpPost]
        public ActionResult TrainStepOne(M_ARABIZIENTRY arabiziEntry)
        {
            // Arabizi to arabic script via direct call to perl script
            var textConverter = new TextConverter();

            // Arabizi to arabic from perl script
            if (arabiziEntry.ArabiziText != null)
            {
                // complete arabizi
                arabiziEntry.ID_ARABIZIENTRY = Guid.NewGuid();

                // prepare darija from perl script
                var arabicText = textConverter.Convert(arabiziEntry.ArabiziText);
                var arabicDarijaEntry = new M_ARABICDARIJAENTRY
                {
                    ID_ARABICDARIJAENTRY = Guid.NewGuid(),
                    ID_ARABIZIENTRY = arabiziEntry.ID_ARABIZIENTRY,
                    ArabicDarijaText = arabicText
                };

                // Save arabiziEntry to Serialization
                String path = Server.MapPath("~/App_Data/data_M_ARABIZIENTRY.txt");
                new TextPersist().Serialize<M_ARABIZIENTRY>(arabiziEntry, path);

                // Save arabicDarijaEntry to Serialization
                path = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY.txt");
                new TextPersist().Serialize<M_ARABICDARIJAENTRY>(arabicDarijaEntry, path);

                // latin words
                MatchCollection matches = TextTools.ExtractLatinWords(arabicDarijaEntry.ArabicDarijaText);

                // save every match
                // also calculate on the fly the number of varaiants
                foreach (Match match in matches)
                {
                    String arabiziWord = match.Value;
                    int variantsCount = new TextConverter().GetAllTranscriptions(arabiziWord).Count;

                    var latinWord = new M_ARABICDARIJAENTRY_LATINWORD
                    {
                        ID_ARABICDARIJAENTRY_LATINWORD = Guid.NewGuid(),
                        ID_ARABICDARIJAENTRY = arabicDarijaEntry.ID_ARABICDARIJAENTRY,
                        LatinWord = arabiziWord,
                        VariantsCount = variantsCount
                    };

                    // Save to Serialization
                    path = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY_LATINWORD.txt");
                    new TextPersist().Serialize<M_ARABICDARIJAENTRY_LATINWORD>(latinWord, path);
                }
            }

            //
            return RedirectToAction("Index");
        }

        /*[HttpPost]
        public ActionResult TrainStepTwo_Tag_LatinWords(M_ARABICDARIJAENTRY arabicDarijaEntry)
        {
            {
                // latin words
                Regex regex = new Regex(@"\p{IsBasicLatin}");
                var matches = regex.Matches(arabicDarijaEntry.ArabicDarijaText);

                // 
                foreach (var match in matches)
                {
                    var latinWord = new M_ARABICDARIJAENTRY_LATINWORD
                    {
                        ID_ARABICDARIJAENTRY = arabicDarijaEntry.ID_ARABICDARIJAENTRY,
                        LatinWord = (String)match
                    };

                    // Save to Serialization
                    String path = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY_LATINWORD.txt");
                    new TextPersist().Serialize<M_ARABICDARIJAENTRY_LATINWORD>(latinWord, path);
                }
            }

            //
            return RedirectToAction("Index");
        }*/

        [HttpPost]
        public ActionResult ArabicDarijaEntryPartialView()
        {
            // deserialize M_ARABICDARIJAENTRY
            List<M_ARABICDARIJAENTRY> entries = new List<M_ARABICDARIJAENTRY>();
            String path = Server.MapPath("~/App_Data/data_" + typeof(M_ARABICDARIJAENTRY).Name + ".txt");
            XmlSerializer serializer = new XmlSerializer(entries.GetType());
            using (var reader = new System.IO.StreamReader(path))
            {
                entries = (List<M_ARABICDARIJAENTRY>)serializer.Deserialize(reader);
            }

            // deserialize M_ARABICDARIJAENTRY_LATINWORD
            List<M_ARABICDARIJAENTRY_LATINWORD> latinWordsEntries = new List<M_ARABICDARIJAENTRY_LATINWORD>();
            path = Server.MapPath("~/App_Data/data_" + typeof(M_ARABICDARIJAENTRY_LATINWORD).Name + ".txt");
            serializer = new XmlSerializer(latinWordsEntries.GetType());
            using (var reader = new System.IO.StreamReader(path))
            {
                latinWordsEntries = (List<M_ARABICDARIJAENTRY_LATINWORD>)serializer.Deserialize(reader);
            }

            // deserialize M_ARABIZIENTRY
            List<M_ARABIZIENTRY> arabiziEntries = new List<M_ARABIZIENTRY>();
            path = Server.MapPath("~/App_Data/data_" + typeof(M_ARABIZIENTRY).Name + ".txt");
            serializer = new XmlSerializer(arabiziEntries.GetType());
            using (var reader = new System.IO.StreamReader(path))
            {
                arabiziEntries = (List<M_ARABIZIENTRY>)serializer.Deserialize(reader);
            }

            //
            List<Class2> xs = new List<Class2>();
            foreach (M_ARABICDARIJAENTRY arabicdarijaentry in entries)
            {
                var perEntryLatinWordsEntries = latinWordsEntries.Where(m => m.ID_ARABICDARIJAENTRY == arabicdarijaentry.ID_ARABICDARIJAENTRY).ToList();
                var x = new Class2
                {
                    ArabicDarijaEntry = arabicdarijaentry,
                    // ArabicDarijaEntryLatinWords = perEntryLatinWordsEntries.Select(m => m.LatinWord).ToList(),
                    ArabicDarijaEntryLatinWords = perEntryLatinWordsEntries,
                    ArabiziEntryText = arabiziEntries.Single(m => m.ID_ARABIZIENTRY == arabicdarijaentry.ID_ARABIZIENTRY).ArabiziText
                };
                xs.Add(x);
            }

            // reverse order
            xs.Reverse();

            // pass entries to partial view via the model (instead of the bag for a view)
            return PartialView("_IndexPartialPage_arabicDarijaEntries", xs);
        }

        [HttpGet]
        public ActionResult X(String arabiziWord, Guid arabiziWordGuid)
        {
            var textConverter = new TextConverter();
            String twinglyApi15Url = "https://data.twingly.net/socialfeed/a/api/v1.5/";
            String twinglyApiUrl = "https://data.twingly.net/socialfeed/a/api/";

            // obtain current twingly key
            // String twinglyApiKey = "2A4CF6A4-4968-46EF-862F-2881EF597A55";
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

                // send size of corpus
                @ViewBag.CorpusSize = new TextFrequency().GetCorpusNumberOfLine();
                @ViewBag.CorpusWordCount = new TextFrequency().GetCorpusWordCount();

                // deserialize & send twingly accounts
                @ViewBag.TwinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));

                return View("Index");
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

                    // send size of corpus
                    @ViewBag.CorpusSize = new TextFrequency().GetCorpusNumberOfLine();
                    @ViewBag.CorpusWordCount = new TextFrequency().GetCorpusWordCount();

                    // deserialize & send twingly accounts
                    @ViewBag.TwinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));

                    return View("Index");
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

                        // send size of corupus
                        @ViewBag.CorpusSize = new TextFrequency().GetCorpusNumberOfLine();
                        @ViewBag.CorpusWordCount = new TextFrequency().GetCorpusWordCount();

                        // deserialize & send twingly accounts
                        @ViewBag.TwinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));

                        return View("Index");
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

                        // send size of corupus
                        @ViewBag.CorpusSize = new TextFrequency().GetCorpusNumberOfLine();
                        @ViewBag.CorpusWordCount = new TextFrequency().GetCorpusWordCount();

                        // deserialize & send twingly accounts
                        @ViewBag.TwinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));

                        return View("Index");
                    }
                }
            }

            // standard behaviour when we are about to find the right variant
            var variants = textConverter.GetAllTranscriptions(arabiziWord);
            if (variants.Count > 50)
            {
                ViewBag.MostPopularVariant = "More than 50 variants";

                // send size of corupus
                @ViewBag.CorpusSize = new TextFrequency().GetCorpusNumberOfLine();
                @ViewBag.CorpusWordCount = new TextFrequency().GetCorpusWordCount();

                // deserialize & send twingly accounts
                @ViewBag.TwinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));

                return View("Index");
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

                    // send size of corupus
                    @ViewBag.CorpusSize = new TextFrequency().GetCorpusNumberOfLine();
                    @ViewBag.CorpusWordCount = new TextFrequency().GetCorpusWordCount();

                    // deserialize & send twingly accounts
                    @ViewBag.TwinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));

                    return View("Index");
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

                    // send size of corupus
                    @ViewBag.CorpusSize = new TextFrequency().GetCorpusNumberOfLine();
                    @ViewBag.CorpusWordCount = new TextFrequency().GetCorpusWordCount();

                    // deserialize & send twingly accounts
                    @ViewBag.TwinglyAccounts = new TextPersist().Deserialize<M_TWINGLYACCOUNT>(Server.MapPath("~/App_Data"));

                    return View("Index");
                }
            }
        }

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
            return View("Index");
        }
    }
}
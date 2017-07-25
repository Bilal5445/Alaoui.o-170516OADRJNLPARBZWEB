using ArabicTextAnalyzer.Business.Provider;
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
                foreach (Match match in matches)
                {
                    var latinWord = new M_ARABICDARIJAENTRY_LATINWORD
                    {
                        ID_ARABICDARIJAENTRY = arabicDarijaEntry.ID_ARABICDARIJAENTRY,
                        LatinWord = match.Value
                    };

                    // Save to Serialization
                    path = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY_LATINWORD.txt");
                    new TextPersist().Serialize<M_ARABICDARIJAENTRY_LATINWORD>(latinWord, path);
                }
            }

            //
            return RedirectToAction("Index");
        }

        [HttpPost]
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
        }

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
            foreach(M_ARABICDARIJAENTRY arabicdarijaentry in entries)
            {
                var perEntryLatinWordsEntries = latinWordsEntries.Where(m => m.ID_ARABICDARIJAENTRY == arabicdarijaentry.ID_ARABICDARIJAENTRY).ToList();
                var x = new Class2
                {
                    ArabicDarijaEntry = arabicdarijaentry,
                    ArabicDarijaEntryLatinWords = perEntryLatinWordsEntries.Select(m => m.LatinWord).ToList(),
                    ArabiziEntryText = arabiziEntries.Single(m => m.ID_ARABIZIENTRY == arabicdarijaentry.ID_ARABIZIENTRY).ArabiziText
                };
                xs.Add(x);
            }

            // reverse order
            xs.Reverse();

            // pass entries to partial view via the model (instead of the bag for a view)
            return PartialView("_IndexPartialPage_arabicDarijaEntries", xs);
        }
    }
}
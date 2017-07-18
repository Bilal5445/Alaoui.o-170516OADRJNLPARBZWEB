using ArabicTextAnalyzer.Business.Provider;
using ArabicTextAnalyzer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            // deserialize
            /*List<M_ARABICDARIJAENTRY> entries = new List<M_ARABICDARIJAENTRY>();
            String path = Server.MapPath("~/App_Data/data_" + typeof(M_ARABICDARIJAENTRY).Name + ".txt");
            XmlSerializer serializer = new XmlSerializer(entries.GetType());
            using (var reader = new System.IO.StreamReader(path))
            {
                entries = (List<M_ARABICDARIJAENTRY>)serializer.Deserialize(reader);
            }
            
            // pass to view bag
            ViewBag.ArabicDarijaEntries = entries;*/

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

                // prepare darija
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
            }

            //
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ArabicDarijaEntryPartialView()
        {
            // deserialize
            List<M_ARABICDARIJAENTRY> entries = new List<M_ARABICDARIJAENTRY>();
            String path = Server.MapPath("~/App_Data/data_" + typeof(M_ARABICDARIJAENTRY).Name + ".txt");
            XmlSerializer serializer = new XmlSerializer(entries.GetType());
            using (var reader = new System.IO.StreamReader(path))
            {
                entries = (List<M_ARABICDARIJAENTRY>)serializer.Deserialize(reader);
            }

            // pass entries to partial view via the model (instead of the bag for a view)
            return PartialView("_IndexPartialPage_arabicDarijaEntries", entries);
        }
    }
}
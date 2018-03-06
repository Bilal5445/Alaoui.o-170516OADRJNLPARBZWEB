using ArabicTextAnalyzer.Business.Provider;
using ArabicTextAnalyzer.Domain;
using ArabicTextAnalyzer.Domain.Models;
using ArabicTextAnalyzer.Models;
using Dapper;
using OADRJNLPCommon.Business;
using OADRJNLPCommon.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using static ArabicTextAnalyzer.Business.Provider.RosetteMultiLanguageDetections;

namespace ArabicTextAnalyzer.BO
{
    public class Arabizer : IDisposable
    {
        public HttpServerUtilityBase Server { get; set; }

        public Arabizer()
        {
        }

        public Arabizer(HttpServerUtilityBase server)
        {
            Server = server;
        }

        public void Dispose()
        {

        }

        #region BACK YARD BO TRAIN
        public dynamic train(M_ARABIZIENTRY arabiziEntry, String mainEntity, Object thisLock = null)
        {
            dynamic expando = new ExpandoObject();
            expando.M_ARABIZIENTRY = arabiziEntry;

            // Arabizi to arabic from perl script
            if (String.IsNullOrWhiteSpace(arabiziEntry.ArabiziText))
            {
                // return Guid.Empty;
                expando.M_ARABICDARIJAENTRY = new M_ARABICDARIJAENTRY
                {
                    ID_ARABICDARIJAENTRY = Guid.Empty
                };
                return expando;
            }

            //
            var id_ARABICDARIJAENTRY = Guid.NewGuid();

            var watch = Stopwatch.StartNew();

            // did we check Fr mode
            var frMode = arabiziEntry.IsFR;

            //
            String arabicText = train_savearabizi(arabiziEntry);

            // Detecte ranges of language with new rosette API 1.9
            List<LanguageRange> languagesRanges = new TextEntityExtraction().GetLanguagesRanges(arabicText);
            if (languagesRanges.Count > 1)
            {
                arabicText = String.Empty;  // reset 
                foreach (LanguageRange langueRange in languagesRanges)
                {
                    LanguageDetection language = new TextEntityExtraction().GetLanguageForRange(langueRange.Region);
                    if (language.language == "fra" /*&& language.confidence > 0.5*/)
                        frMode = true;
                    if (language.language == "eng" && language.confidence > 0.7)    // it thinks arabizi is 'eng' with 0.65 confid
                        frMode = true;

                    String larabicText = langueRange.Region;

                    if (frMode == false)
                        larabicText = new TextConverter().Preprocess_upstream(larabicText);

                    // mark as ignore : url 
                    larabicText = train_markAsIgnore(larabicText);

                    if (frMode == false)
                        larabicText = train_bidict(larabicText);

                    if (frMode == false)
                        larabicText = train_binggoogle(larabicText);

                    if (frMode == false)
                        larabicText = train_perl(watch, larabicText);

                    arabicText += larabicText;
                }
            }
            else
            {
                LanguageDetection language = languagesRanges[0].Language;
                if (language.language == "fra" /*&& language.confidence > 0.5*/)
                    frMode = true;
                if (language.language == "eng" && language.confidence > 0.7)    // it thinks arabizi is 'eng' with 0.65 confid
                    frMode = true;

                String larabicText = languagesRanges[0].Region;

                if (frMode == false)
                    larabicText = new TextConverter().Preprocess_upstream(larabicText);

                // mark as ignore : url 
                larabicText = train_markAsIgnore(larabicText);

                if (frMode == false)
                    larabicText = train_bidict(larabicText);

                if (frMode == false)
                    larabicText = train_binggoogle(larabicText);

                if (frMode == false)
                    larabicText = train_perl(watch, larabicText);

                arabicText = larabicText;
            }

            //
            var arabicDarijaEntry = new M_ARABICDARIJAENTRY
            {
                ID_ARABICDARIJAENTRY = id_ARABICDARIJAENTRY,
                ID_ARABIZIENTRY = arabiziEntry.ID_ARABIZIENTRY,
                ArabicDarijaText = arabicText
            };

            // save to persist
            saveserializeM_ARABICDARIJAENTRY_EFSQL(arabicDarijaEntry);

            //
            arabicText = arabicDarijaEntry.ArabicDarijaText;
            expando.M_ARABICDARIJAENTRY = arabicDarijaEntry;

            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = train_savener(arabicText, id_ARABICDARIJAENTRY, AccessMode.efsql);
            expando.M_ARABICDARIJAENTRY_TEXTENTITYs = textEntities;

            // apply main tag : add main entity & Save to Serialization
            if (!String.IsNullOrEmpty(mainEntity))
            {
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

            // all is good so far
            expando.status = true;

            //
            return expando;
        }

        public dynamic train_uow(M_ARABIZIENTRY arabiziEntry, String mainEntity, ArabiziDbContext db, bool isEndOfScope = false)
        {
            dynamic expando = new ExpandoObject();
            expando.M_ARABIZIENTRY = arabiziEntry;

            // Arabizi to arabic from perl script
            if (String.IsNullOrWhiteSpace(arabiziEntry.ArabiziText))
            {
                // return Guid.Empty;
                expando.M_ARABICDARIJAENTRY = new M_ARABICDARIJAENTRY
                {
                    ID_ARABICDARIJAENTRY = Guid.Empty
                };
                return expando;
            }

            //
            var id_ARABICDARIJAENTRY = Guid.NewGuid();

            var watch = Stopwatch.StartNew();

            // did we check Fr mode
            var frMode = arabiziEntry.IsFR;

            //
            String arabicText = train_savearabizi_uow(arabiziEntry, db, isEndOfScope: false);

            // Detecte ranges of language with new rosette API 1.9
            List<LanguageRange> languagesRanges = new TextEntityExtraction().GetLanguagesRanges(arabicText);
            if (languagesRanges.Count > 1)
            {
                arabicText = String.Empty;  // reset 
                foreach (LanguageRange langueRange in languagesRanges)
                {
                    LanguageDetection language = new TextEntityExtraction().GetLanguageForRange(langueRange.Region);
                    if (language.language == "fra" /*&& language.confidence > 0.5*/)
                        frMode = true;
                    if (language.language == "eng" && language.confidence > 0.7)    // it thinks arabizi is 'eng' with 0.65 confid
                        frMode = true;

                    String larabicText = langueRange.Region;

                    if (frMode == false)
                        larabicText = new TextConverter().Preprocess_upstream(larabicText);

                    // mark as ignore : url 
                    larabicText = train_markAsIgnore(larabicText);

                    if (frMode == false)
                        larabicText = train_bidict(larabicText);

                    if (frMode == false)
                        larabicText = train_binggoogle(larabicText);

                    if (frMode == false)
                        larabicText = train_perl(watch, larabicText);

                    arabicText += larabicText;
                }
            }
            else
            {
                LanguageDetection language = languagesRanges[0].Language;
                if (language.language == "fra" /*&& language.confidence > 0.5*/)
                    frMode = true;
                if (language.language == "eng" && language.confidence > 0.7)    // it thinks arabizi is 'eng' with 0.65 confid
                    frMode = true;

                String larabicText = languagesRanges[0].Region;

                if (frMode == false)
                    larabicText = new TextConverter().Preprocess_upstream(larabicText);

                // mark as ignore : url 
                larabicText = train_markAsIgnore(larabicText);

                if (frMode == false)
                    larabicText = train_bidict(larabicText);

                if (frMode == false)
                    larabicText = train_binggoogle(larabicText);

                if (frMode == false)
                    larabicText = train_perl(watch, larabicText);

                arabicText = larabicText;
            }

            //
            var arabicDarijaEntry = new M_ARABICDARIJAENTRY
            {
                ID_ARABICDARIJAENTRY = id_ARABICDARIJAENTRY,
                ID_ARABIZIENTRY = arabiziEntry.ID_ARABIZIENTRY,
                ArabicDarijaText = arabicText
            };

            // save to persist
            saveserializeM_ARABICDARIJAENTRY_EFSQL_uow(arabicDarijaEntry, db, isEndOfScope: false);

            //
            arabicText = arabicDarijaEntry.ArabicDarijaText;
            expando.M_ARABICDARIJAENTRY = arabicDarijaEntry;

            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = train_savener_uow(arabicText, id_ARABICDARIJAENTRY, db, isEndOfScope: false);
            expando.M_ARABICDARIJAENTRY_TEXTENTITYs = textEntities;

            // apply main tag : add main entity & Save to Serialization
            if (!String.IsNullOrEmpty(mainEntity))
            {
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
                saveserializeM_ARABICDARIJAENTRY_TEXTENTITY_EFSQL_uow(textEntity, db, isEndOfScope: false);
            }

            //
            return expando;
        }

        private String train_savearabizi(M_ARABIZIENTRY arabiziEntry/*, AccessMode accessMode*/)
        {
            // complete arabizi entry & Save arabiziEntry to Serialization
            arabiziEntry.ID_ARABIZIENTRY = Guid.NewGuid();

            // clean
            arabiziEntry.ArabiziText = arabiziEntry.ArabiziText.Trim(new char[] { ' ', '\t' });

            // save
            saveserializeM_ARABIZIENTRY_EFSQL(arabiziEntry/*, accessMode*/);

            //
            return arabiziEntry.ArabiziText;
        }

        private String train_savearabizi_uow(M_ARABIZIENTRY arabiziEntry, ArabiziDbContext db, bool isEndOfScope = false)
        {
            // complete arabizi entry & Save arabiziEntry to Serialization
            arabiziEntry.ID_ARABIZIENTRY = Guid.NewGuid();

            // clean
            arabiziEntry.ArabiziText = arabiziEntry.ArabiziText.Trim(new char[] { ' ', '\t' });

            // save
            saveserializeM_ARABIZIENTRY_EFSQL_uow(arabiziEntry, db, isEndOfScope);

            //
            return arabiziEntry.ArabiziText;
        }

        private static string train_bidict(string arabicText)
        {
            // clean before google/bing : o w
            arabicText = new TextFrequency().ReplaceArzByArFromBidict(arabicText);
            return arabicText;
        }

        private static string train_markAsIgnore(string arabicText)
        {
            // clean before google/bing : url
            arabicText = new TextFrequency().MarkAsIgnore_URL(arabicText);
            return arabicText;
        }

        private /*static*/ string train_binggoogle(String arabicText)
        {
            // first pass : correct/translate the original arabizi into msa arabic using big/google apis (to take care of french/english segments in codeswitch arabizi posts)
            return new TranslationTools(Server).CorrectTranslate(arabicText);
        }

        private M_ARABICDARIJAENTRY train_saveperl(Stopwatch watch, string arabicText, Guid id_ARABIZIENTRY, Guid id_ARABICDARIJAENTRY, AccessMode accessMode, bool frMode = false)
        {
            if (frMode == false)
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
                arabicText = new TextConverter().Convert(Server, watch, arabicText);

                // restore do not translate from 001000100
                var regex2 = new Regex(@"001000100");
                foreach (Match match in matches)
                {
                    arabicText = regex2.Replace(arabicText, match.Value, 1);
                }
                // clean <span class='notranslate'>
                arabicText = new Regex(@"<span class='notranslate'>(.*?)</span>").Replace(arabicText, "$1");
            }

            //
            var arabicDarijaEntry = new M_ARABICDARIJAENTRY
            {
                ID_ARABICDARIJAENTRY = id_ARABICDARIJAENTRY,
                ID_ARABIZIENTRY = id_ARABIZIENTRY,
                ArabicDarijaText = arabicText
            };

            // save to persist
            saveserializeM_ARABICDARIJAENTRY_EFSQL(arabicDarijaEntry);

            //
            return arabicDarijaEntry;
        }

        private String train_perl(Stopwatch watch, string arabicText)
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
            arabicText = new TextConverter().Convert(Server, watch, arabicText);

            // restore do not translate from 001000100
            var regex2 = new Regex(@"001000100");
            foreach (Match match in matches)
            {
                arabicText = regex2.Replace(arabicText, match.Value, 1);
            }
            // clean <span class='notranslate'>
            arabicText = new Regex(@"<span class='notranslate'>(.*?)</span>").Replace(arabicText, "$1");

            //
            return arabicText;
        }

        private M_ARABICDARIJAENTRY train_saveperl_uow(Stopwatch watch, string arabicText, Guid id_ARABIZIENTRY, Guid id_ARABICDARIJAENTRY, ArabiziDbContext db, bool isEndOfScope = false, bool frMode = false)
        {
            if (frMode == false)
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
                arabicText = new TextConverter().Convert(Server, watch, arabicText);

                // restore do not translate from 001000100
                var regex2 = new Regex(@"001000100");
                foreach (Match match in matches)
                {
                    arabicText = regex2.Replace(arabicText, match.Value, 1);
                }
                // clean <span class='notranslate'>
                arabicText = new Regex(@"<span class='notranslate'>(.*?)</span>").Replace(arabicText, "$1");
            }

            //
            var arabicDarijaEntry = new M_ARABICDARIJAENTRY
            {
                ID_ARABICDARIJAENTRY = id_ARABICDARIJAENTRY,
                ID_ARABIZIENTRY = id_ARABIZIENTRY,
                ArabicDarijaText = arabicText
            };

            // save to persist
            saveserializeM_ARABICDARIJAENTRY_EFSQL_uow(arabicDarijaEntry, db, isEndOfScope: isEndOfScope);

            //
            return arabicDarijaEntry;
        }

        private List<M_ARABICDARIJAENTRY_LATINWORD> train_savelatinwords(String arabicText, Guid id_ARABICDARIJAENTRY, AccessMode accessMode)
        {
            List<M_ARABICDARIJAENTRY_LATINWORD> arabicDarijaEntryLatinWords = new List<M_ARABICDARIJAENTRY_LATINWORD>();

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

                //
                arabicDarijaEntryLatinWords.Add(latinWord);

                // Save to Serialization
                saveserializeM_ARABICDARIJAENTRY_LATINWORD_EFSQL(latinWord);
            }

            return arabicDarijaEntryLatinWords;
        }

        private List<M_ARABICDARIJAENTRY_LATINWORD> train_savelatinwords_uow(String arabicText, Guid id_ARABICDARIJAENTRY, ArabiziDbContext db, bool isEndOfScope = false)
        {
            List<M_ARABICDARIJAENTRY_LATINWORD> arabicDarijaEntryLatinWords = new List<M_ARABICDARIJAENTRY_LATINWORD>();

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

                //
                arabicDarijaEntryLatinWords.Add(latinWord);

                // Save to Serialization
                saveserializeM_ARABICDARIJAENTRY_LATINWORD_EFSQL_uow(latinWord, db, isEndOfScope: isEndOfScope);
            }

            return arabicDarijaEntryLatinWords;
        }

        /*private void train_savesa(string arabicText)
        {
            // Sentiment analysis from watson https://gateway.watsonplatform.net/" and Save to Serialization
            var textSentimentAnalyzer = new TextSentimentAnalyzer();
            TextSentiment sentiment = textSentimentAnalyzer.GetSentiment(arabicText);

            //
            saveserializeM_ARABICDARIJAENTRY_LATINWORD_XML(sentiment);
        }*/

        private List<M_ARABICDARIJAENTRY_TEXTENTITY> train_savener(string arabicText, Guid id_ARABICDARIJAENTRY, AccessMode accessMode)
        {
            // Entity extraction from rosette (https://api.rosette.com/rest/v1/)
            var entities = new TextEntityExtraction().GetEntities(arabicText);

            // NER manual extraction
            return new TextEntityExtraction().NerManualExtraction(arabicText, entities, id_ARABICDARIJAENTRY, saveserializeM_ARABICDARIJAENTRY_TEXTENTITY, accessMode);
        }

        private List<M_ARABICDARIJAENTRY_TEXTENTITY> train_savener_uow(string arabicText, Guid id_ARABICDARIJAENTRY, ArabiziDbContext db, bool isEndOfScope = false)
        {
            // Entity extraction from rosette (https://api.rosette.com/rest/v1/)
            var entities = new TextEntityExtraction().GetEntities(arabicText);

            // NER manual extraction
            return new TextEntityExtraction().NerManualExtraction_uow(arabicText, entities, id_ARABICDARIJAENTRY, saveserializeM_ARABICDARIJAENTRY_TEXTENTITY_EFSQL_uow, db, isEndOfScope);
        }
        #endregion

        #region BACK YARD BO SAVE / DELETE
        private void saveserializeM_ARABIZIENTRY_EFSQL(M_ARABIZIENTRY arabiziEntry)
        {
            using (var db = new ArabiziDbContext())
            {
                db.M_ARABIZIENTRYs.Add(arabiziEntry);

                // commit
                db.SaveChanges();
            }
        }

        private void saveserializeM_ARABIZIENTRY_EFSQL_uow(M_ARABIZIENTRY arabiziEntry, ArabiziDbContext db, bool isEndOfScope = false)
        {
            // using (var db = new ArabiziDbContext())
            // {
            db.M_ARABIZIENTRYs.Add(arabiziEntry);

            // commit
            if (isEndOfScope == true)
                db.SaveChanges();
            // }
        }

        public void saveserializeM_ARABICDARIJAENTRY_EFSQL(M_ARABICDARIJAENTRY arabicDarijaEntry)
        {
            using (var db = new ArabiziDbContext())
            {
                db.M_ARABICDARIJAENTRYs.Add(arabicDarijaEntry);

                // commit
                db.SaveChanges();
            }
        }

        private void saveserializeM_ARABICDARIJAENTRY_EFSQL_uow(M_ARABICDARIJAENTRY arabicDarijaEntry, ArabiziDbContext db, bool isEndOfScope = false)
        {
            // using (var db = new ArabiziDbContext())
            // {
            db.M_ARABICDARIJAENTRYs.Add(arabicDarijaEntry);

            // commit
            if (isEndOfScope == true)
                db.SaveChanges();
            // }
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

        private void saveserializeM_ARABICDARIJAENTRY_LATINWORD_EFSQL_uow(M_ARABICDARIJAENTRY_LATINWORD latinWord, ArabiziDbContext db, bool isEndOfScope = false)
        {
            // using (var db = new ArabiziDbContext())
            // {
            db.M_ARABICDARIJAENTRY_LATINWORDs.Add(latinWord);

            // commit
            if (isEndOfScope == true)
                db.SaveChanges();
            // }
        }

        private void saveserializeM_ARABICDARIJAENTRY_TEXTENTITY(M_ARABICDARIJAENTRY_TEXTENTITY textEntity, AccessMode accessMode)
        {
            if (accessMode == AccessMode.efsql)
                saveserializeM_ARABICDARIJAENTRY_TEXTENTITY_EFSQL(textEntity);
        }

        public void saveserializeM_ARABICDARIJAENTRY_TEXTENTITY_EFSQL(M_ARABICDARIJAENTRY_TEXTENTITY textEntity)
        {
            using (var db = new ArabiziDbContext())
            {
                db.M_ARABICDARIJAENTRY_TEXTENTITYs.Add(textEntity);

                // commit
                db.SaveChanges();
            }
        }

        public void saveserializeM_ARABICDARIJAENTRY_TEXTENTITY_EFSQL_uow(M_ARABICDARIJAENTRY_TEXTENTITY textEntity, ArabiziDbContext db, bool isEndOfScope = false)
        {
            // using (var db = new ArabiziDbContext())
            // {
            db.M_ARABICDARIJAENTRY_TEXTENTITYs.Add(textEntity);

            // commit
            if (isEndOfScope == true)
                db.SaveChanges();
            // }
        }

        public void serialize_Delete_M_ARABICDARIJAENTRY_TEXTENTITY_EFSQL(M_ARABICDARIJAENTRY_TEXTENTITY textEntity)
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

        public void serialize_Delete_M_ARABICDARIJAENTRY_TEXTENTITY_EFSQL(List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities)
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

        public void saveserializeM_XTRCTTHEME_EFSQL(M_XTRCTTHEME m_xtrcttheme)
        {
            using (var db = new ArabiziDbContext())
            {
                db.M_XTRCTTHEMEs.Add(m_xtrcttheme);

                // commit
                db.SaveChanges();
            }
        }

        public void saveserializeM_XTRCTTHEME_KEYWORDs_EFSQL(M_XTRCTTHEME_KEYWORD m_xtrcttheme_keyword)
        {
            using (var db = new ArabiziDbContext())
            {
                //
                db.M_XTRCTTHEME_KEYWORDs.Add(m_xtrcttheme_keyword);

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

        public void saveserializeM_XTRCTTHEME_EFSQL_Active(String themename, String userId)
        {
            using (var db = new ArabiziDbContext())
            {
                var xtrctThemes = db.M_XTRCTTHEMEs;

                var tobeactiveXtrctTheme = xtrctThemes.Where(m => m.ThemeName == themename && m.UserID == userId).FirstOrDefault<M_XTRCTTHEME>();
                tobeactiveXtrctTheme.CurrentActive = "active";

                // commit
                db.SaveChanges();
            }
        }

        public void saveserializeM_XTRCTTHEME_EFSQL_Deactivate(String userId)
        {
            using (var db = new ArabiziDbContext())
            {
                var xtrctThemes = db.M_XTRCTTHEMEs;

                var tobeactiveXtrctTheme = xtrctThemes.Where(m => m.CurrentActive == "active" && m.UserID == userId).FirstOrDefault<M_XTRCTTHEME>();
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

        public void Serialize_Delete_M_ARABIZIENTRY_Cascading_EFSQL_uow(Guid id_arabizientry, ArabiziDbContext db, bool isEndOfScope = false)
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
            if (isEndOfScope == true)
                db.SaveChanges();
        }

        public dynamic Serialize_SoftDelete_M_XTRCTTHEME_Cascading_EFSQL(Guid idXtrctTheme)
        {
            // cascade but working upside down

            using (var db = new ArabiziDbContext())
            {
                dynamic expando = new ExpandoObject();

                // get theme
                var xtrctTheme = db.M_XTRCTTHEMEs.Single(m => m.ID_XTRCTTHEME == idXtrctTheme);

                // save associated user
                var userId = xtrctTheme.UserID;

                // check before : should not be the default
                if (xtrctTheme.ThemeName == "Default")
                {
                    expando.Result = false;
                    expando.ErrMessage = "Default theme cannot be deleted";
                    return expando;
                }

                // get associated theme keywords
                var xtrctThemeKeywords = db.M_XTRCTTHEME_KEYWORDs.Where(m => m.ID_XTRCTTHEME == idXtrctTheme);

                // associated theme keywords can be (soft)deleted
                foreach (var xtrctThemeKeyword in xtrctThemeKeywords) xtrctThemeKeyword.IsDeleted = 1;

                // get associated arabizi entries
                var arabiziEntrys = db.M_ARABIZIENTRYs.Where(m => m.ID_XTRCTTHEME == idXtrctTheme);

                // get associated arabic darija items
                var arabicDarijaEntrys = db.M_ARABICDARIJAENTRYs.Join(arabiziEntrys, j => j.ID_ARABIZIENTRY, z => z.ID_ARABIZIENTRY, (j, z) => j);

                // get associated text entities
                var arabicDarijaEntryTextEntitys = db.M_ARABICDARIJAENTRY_TEXTENTITYs.Join(arabicDarijaEntrys, te => te.ID_ARABICDARIJAENTRY, j => j.ID_ARABICDARIJAENTRY, (te, j) => te);

                // associated text entities can be (soft)deleted
                foreach (var arabicDarijaEntryTextEntity in arabicDarijaEntryTextEntitys) arabicDarijaEntryTextEntity.IsDeleted = 1;

                // get associated latin word
                var arabicDarijaEntryLatinWords = db.M_ARABICDARIJAENTRY_LATINWORDs.Join(arabicDarijaEntrys, lw => lw.ID_ARABICDARIJAENTRY, j => j.ID_ARABICDARIJAENTRY, (lw, j) => lw);

                // associated latin word can be (soft)deleted
                foreach (var arabicDarijaEntryLatinWord in arabicDarijaEntryLatinWords) arabicDarijaEntryLatinWord.IsDeleted = 1;

                // associated arabic darija items can be (soft)deleted
                foreach (var arabicDarijaEntry in arabicDarijaEntrys) arabicDarijaEntry.IsDeleted = 1;

                // associated arabizi entries can be (soft)deleted
                foreach (var arabiziEntry in arabiziEntrys) arabiziEntry.IsDeleted = 1;

                // theme can be (soft)deleted
                xtrctTheme.IsDeleted = 1;

                // de activate theme
                xtrctTheme.CurrentActive = String.Empty;

                // set default back to active
                var defaultXtrctTheme = db.M_XTRCTTHEMEs.Single(m => m.ThemeName == "Default" && m.UserID == userId);
                defaultXtrctTheme.CurrentActive = "active";

                // commit
                db.SaveChanges();

                // return success
                expando.Result = true;
                return expando;
            }
        }

        public dynamic Serialize_Delete_M_XTRCTTHEME_Cascading_EFSQL(Guid idXtrctTheme)
        {
            // cascade but working upside down

            using (var db = new ArabiziDbContext())
            {
                dynamic expando = new ExpandoObject();

                // get theme
                var xtrctTheme = db.M_XTRCTTHEMEs.Single(m => m.ID_XTRCTTHEME == idXtrctTheme);

                // save associated user
                var userId = xtrctTheme.UserID;

                // check before : should not be the default
                if (xtrctTheme.ThemeName == "Default")
                {
                    expando.Result = false;
                    expando.ErrMessage = "Default theme cannot be deleted";
                    return expando;
                }

                // get associated theme keywords
                var xtrctThemeKeywords = db.M_XTRCTTHEME_KEYWORDs.Where(m => m.ID_XTRCTTHEME == idXtrctTheme);

                // associated theme keywords can be deleted
                db.M_XTRCTTHEME_KEYWORDs.RemoveRange(xtrctThemeKeywords);

                // get associated arabizi entries
                var arabiziEntrys = db.M_ARABIZIENTRYs.Where(m => m.ID_XTRCTTHEME == idXtrctTheme);

                // get associated arabic darija items
                var arabicDarijaEntrys = db.M_ARABICDARIJAENTRYs.Join(arabiziEntrys, j => j.ID_ARABIZIENTRY, z => z.ID_ARABIZIENTRY, (j, z) => j);

                // get associated text entities
                var arabicDarijaEntryTextEntitys = db.M_ARABICDARIJAENTRY_TEXTENTITYs.Join(arabicDarijaEntrys, te => te.ID_ARABICDARIJAENTRY, j => j.ID_ARABICDARIJAENTRY, (te, j) => te);

                // associated text entities can be deleted
                db.M_ARABICDARIJAENTRY_TEXTENTITYs.RemoveRange(arabicDarijaEntryTextEntitys);

                // get associated latin word
                var arabicDarijaEntryLatinWords = db.M_ARABICDARIJAENTRY_LATINWORDs.Join(arabicDarijaEntrys, lw => lw.ID_ARABICDARIJAENTRY, j => j.ID_ARABICDARIJAENTRY, (lw, j) => lw);

                // associated latin word can be deleted
                db.M_ARABICDARIJAENTRY_LATINWORDs.RemoveRange(arabicDarijaEntryLatinWords);

                // associated arabic darija items can be deleted
                db.M_ARABICDARIJAENTRYs.RemoveRange(arabicDarijaEntrys);

                // associated arabizi entries can be deleted
                db.M_ARABIZIENTRYs.RemoveRange(arabiziEntrys);

                // theme can be deleted
                db.M_XTRCTTHEMEs.Remove(xtrctTheme);

                // set default back to active
                var defaultXtrctTheme = db.M_XTRCTTHEMEs.Single(m => m.ThemeName == "Default" && m.UserID == userId);
                defaultXtrctTheme.CurrentActive = "active";

                // commit
                db.SaveChanges();

                // return success
                expando.Result = true;
                return expando;
            }
        }
        #endregion

        #region BACK YARD BO LOAD XTRCTTHEME
        public List<LM_CountPerUser> loaddeserializeM_XTRCTTHEME_CountPerUser_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT UserID, COUNT(*) CountPerUser FROM T_XTRCTTHEME GROUP BY UserID ";

                conn.Open();
                return conn.Query<LM_CountPerUser>(qry).ToList();
            }
        }

        public List<M_XTRCTTHEME> loaddeserializeM_XTRCTTHEME_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_XTRCTTHEME ORDER BY ThemeName ";

                conn.Open();
                return conn.Query<M_XTRCTTHEME>(qry).ToList();
            }
        }

        public List<M_XTRCTTHEME> loaddeserializeM_XTRCTTHEME_DAPPERSQL(String userId)
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_XTRCTTHEME WHERE UserID = '" + userId + "' ORDER BY ThemeName ";

                conn.Open();
                return conn.Query<M_XTRCTTHEME>(qry).ToList();
            }
        }

        public M_XTRCTTHEME loadDeserializeM_XTRCTTHEME_Active_DAPPERSQL(String userId)
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_XTRCTTHEME WHERE CurrentActive = 'active' AND UserID = '" + userId + "'";

                conn.Open();
                return conn.QueryFirst<M_XTRCTTHEME>(qry);
            }
        }

        public M_XTRCTTHEME loadDeserializeM_XTRCTTHEME_DAPPERSQL(Guid idXtrctTheme)
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_XTRCTTHEME WHERE ID_XTRCTTHEME = '" + idXtrctTheme + "'";

                conn.Open();
                return conn.QueryFirst<M_XTRCTTHEME>(qry);
            }
        }
        #endregion

        #region BACK YARD BO LOAD REGISTER_APPS_USERS
        public List<RegisterApp> loaddeserializeRegisterApp_DAPPERSQL(String userId)
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM RegisterApps WHERE UserID = '" + userId + "' ";

                conn.Open();
                return conn.Query<RegisterApp>(qry).ToList();
            }
        }

        public List<RegisterApp> loaddeserializeRegisterApps_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM RegisterApps ";

                conn.Open();
                return conn.Query<RegisterApp>(qry).ToList();
            }
        }

        public List<RegisterUser> loaddeserializeRegisterUsers_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM RegisterUser ";

                conn.Open();
                return conn.Query<RegisterUser>(qry).ToList();
            }
        }
        #endregion

        #region BACK YARD BO LOAD FB_PAGES_POSTS_COMMENTS
        // Get Influencer details on the influencerId 
        public T_FB_INFLUENCER loadDeserializeT_FB_INFLUENCER_DAPPERSQL(string influencerid, Guid themeid)
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT * FROM T_FB_INFLUENCER "
                        + "WHERE id = '" + influencerid + "' "
                        + "AND fk_theme = '" + themeid + "'";

                //
                conn.Open();
                return conn.QuerySingle<T_FB_INFLUENCER>(qry);
            }
        }

        public List<T_FB_INFLUENCER> loadAllT_Fb_InfluencerAsTheme_DAPPERSQL(String userId)
        {
            //
            var t_fb_Influencer = new List<T_FB_INFLUENCER>();
            var themes = new Arabizer().loadDeserializeM_XTRCTTHEME_Active_DAPPERSQL(userId);
            M_XTRCTTHEME theme = (themes != null) ? themes : new M_XTRCTTHEME();
            if (theme.ID_XTRCTTHEME != null)
            {
                String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    String qry = "SELECT * FROM T_FB_INFLUENCER WHERE fk_theme = '" + theme.ID_XTRCTTHEME + "'";

                    conn.Open();
                    return conn.Query<T_FB_INFLUENCER>(qry).ToList();
                }
            }
            else
            {
                return t_fb_Influencer;
            }
        }

        public List<LM_CountPerTheme> loaddeserializeT_FB_INFLUENCER_CountPerTheme_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry = "SELECT fk_theme, COUNT(*) CountPerTheme FROM T_FB_INFLUENCER GROUP BY fk_theme ";

                conn.Open();
                return conn.Query<LM_CountPerTheme>(qry).ToList();
            }
        }

        public List<FB_POST> loaddeserializeT_FB_POST_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                //
                String qry0 = "SELECT * "
                            + "FROM T_FB_POST ";

                //
                qry0 += "ORDER BY date_publishing DESC ";

                //
                conn.Open();
                return conn.Query<FB_POST>(qry0).ToList();
            }
        }

        public List<FB_POST> loaddeserializeT_FB_POST_DAPPERSQL(string influencerid, bool isForSendMail = false)
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                //
                String qry0 = "SELECT * "
                            + "FROM T_FB_POST "
                            + "WHERE fk_influencer = '" + influencerid + "' ";

                //
                if (isForSendMail == true)
                    qry0 += "AND MailBody IS NOT NULL AND (NoOfTimeMailSend IS NULL OR NoOfTimeMailSend < 2) ";

                //
                qry0 += "ORDER BY date_publishing DESC ";

                //
                conn.Open();
                return conn.Query<FB_POST>(qry0).ToList();
            }
        }

        public List<LM_CountPerTheme> loaddeserializeT_FB_POST_CountPerTheme_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry0 = "SELECT I.fk_theme, COUNT(*) CountPerTheme "
                            + "FROM T_FB_POST P "
                            + "INNER JOIN T_FB_INFLUENCER I ON P.fk_influencer = I.id "
                            + "GROUP BY I.fk_theme ";

                conn.Open();
                return conn.Query<LM_CountPerTheme>(qry0).ToList();
            }
        }

        public List<FBFeedComment> loaddeserializeT_FB_Comments_DAPPERSQL(string postid = "")
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                //
                String qry = "SELECT * FROM FBFeedComments ";

                //
                if (!string.IsNullOrEmpty(postid))
                    qry += "WHERE id LIKE '" + postid + "%' ";

                //
                qry += "ORDER BY created_time DESC ";

                conn.Open();
                return conn.Query<FBFeedComment>(qry).ToList();
            }
        }

        public List<LM_CountPerTheme> loaddeserializeT_FB_Comments_CountPerTheme_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ScrapyWebEntities"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry0 = "SELECT I.fk_theme, COUNT(*) CountPerTheme "
                            + "FROM FBFeedComments C "
                            + "INNER JOIN T_FB_POST P ON C.feedId = P.id "
                            + "INNER JOIN T_FB_INFLUENCER I ON P.fk_influencer = I.id "
                            + "GROUP BY I.fk_theme ";

                conn.Open();
                return conn.Query<LM_CountPerTheme>(qry0).ToList();
            }
        }
        #endregion

        #region BACK YARD BO LOAD ARZ_AR_ENTRIES
        public List<LM_CountPerUser> loaddeserializeM_ARABICDARIJAENTRY_CountPerUser_DAPPERSQL()
        {
            String ConnectionString = ConfigurationManager.ConnectionStrings["ConnLocalDBArabizi"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                String qry0 = "SELECT XT.UserID, COUNT(*) CountPerUser FROM T_ARABICDARIJAENTRY AR "
                            + "INNER JOIN T_ARABIZIENTRY ARZ ON AR.ID_ARABIZIENTRY = ARZ.ID_ARABIZIENTRY "
                            + "INNER JOIN T_XTRCTTHEME XT ON ARZ.ID_XTRCTTHEME = XT.ID_XTRCTTHEME "
                            + "GROUP BY XT.UserID ";

                conn.Open();
                return conn.Query<LM_CountPerUser>(qry0).ToList();
            }
        }

        public M_ARABICDARIJAENTRY loaddeserializeM_ARABICDARIJAENTRY_DB_by_ArabiziEntryId(Guid arabiziWordGuid)
        {
            using (var db = new ArabiziDbContext())
            {
                return db.M_ARABICDARIJAENTRYs.Where(m => m.ID_ARABIZIENTRY == arabiziWordGuid).SingleOrDefault();
            }
        }

        public M_ARABICDARIJAENTRY loaddeserializeM_ARABICDARIJAENTRY_DB_by_ArabicDarijaEntryId(Guid arabicDarijaEntryGuid)
        {
            using (var db = new ArabiziDbContext())
            {
                return db.M_ARABICDARIJAENTRYs.Where(m => m.ID_ARABICDARIJAENTRY == arabicDarijaEntryGuid).SingleOrDefault();
            }
        }
        #endregion
    }
}
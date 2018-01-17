using ArabicTextAnalyzer.Business.Provider;
using ArabicTextAnalyzer.Domain;
using ArabicTextAnalyzer.Domain.Models;
using ArabicTextAnalyzer.Models;
using Dapper;
using OADRJNLPCommon.Business;
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
    public class Arabizer
    {
        public HttpServerUtilityBase Server { get; set; }

        public Arabizer()
        {
        }

        public Arabizer(HttpServerUtilityBase server)
        {
            Server = server;
        }

        #region BACK YARD BO TRAIN
        public dynamic train(M_ARABIZIENTRY arabiziEntry, String mainEntity, Object thisLock = null)
        {
            dynamic expando = new ExpandoObject();
            expando.M_ARABIZIENTRY = arabiziEntry;

            // Arabizi to arabic from perl script
            if (arabiziEntry.ArabiziText == null)
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
            // List<String> languagesRanges = new TextEntityExtraction().GetLanguagesRanges(arabicText);
            List<LanguageRange> languagesRanges = new TextEntityExtraction().GetLanguagesRanges(arabicText);
            if (languagesRanges.Count > 1)
            {

                arabicText = String.Empty; // reset 
                                           // foreach (String langueRange in languagesRanges)
                foreach (LanguageRange langueRange in languagesRanges)
                {
                    // LanguageDetection language = new TextEntityExtraction().GetLanguageForRange(langueRange);
                    LanguageDetection language = new TextEntityExtraction().GetLanguageForRange(langueRange.Region);
                    if (language.language == "fra" /*&& language.confidence > 0.5*/)
                        frMode = true;
                    if (language.language == "eng" && language.confidence > 0.7)    // it thinks arabizi is 'eng' with 0.65 confid
                        frMode = true;

                    // String larabicText = langueRange;
                    String larabicText = langueRange.Region;

                    if (frMode == false)
                        larabicText = new TextConverter().Preprocess_upstream(larabicText);

                    // mark as ignore : url 
                    larabicText = train_markAsIgnore(larabicText);

                    if (frMode == false)
                        larabicText = train_bidict(larabicText);

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

                // String larabicText = langueRange;
                String larabicText = languagesRanges[0].Region;

                if (frMode == false)
                    larabicText = new TextConverter().Preprocess_upstream(larabicText);

                // mark as ignore : url 
                arabicText = train_markAsIgnore(arabicText);

                if (frMode == false)
                    arabicText = train_bidict(arabicText);

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
            /*if (frMode == false)
                arabicText = new TextConverter().Preprocess_upstream(arabicText);

            // mark as ignore : url 
            arabicText = train_markAsIgnore(arabicText);

            if (frMode == false)
                arabicText = train_bidict(arabicText);

            if (frMode == false)
                arabicText = train_binggoogle(arabicText);

            var arabicDarijaEntry = train_saveperl(watch, arabicText, arabiziEntry.ID_ARABIZIENTRY, id_ARABICDARIJAENTRY, AccessMode.efsql, frMode);*/

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

            //
            return expando;
        }

        public dynamic train_uow(M_ARABIZIENTRY arabiziEntry, String mainEntity, ArabiziDbContext db, bool isEndOfScope = false)
        {
            dynamic expando = new ExpandoObject();
            expando.M_ARABIZIENTRY = arabiziEntry;

            // Arabizi to arabic from perl script
            if (arabiziEntry.ArabiziText == null)
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
            // List<String> languagesRanges = new TextEntityExtraction().GetLanguagesRanges(arabicText);
            List<LanguageRange> languagesRanges = new TextEntityExtraction().GetLanguagesRanges(arabicText);
            if (languagesRanges.Count > 1)
            {
                arabicText = String.Empty; // reset 
                // foreach (String langueRange in languagesRanges)
                foreach (LanguageRange langueRange in languagesRanges)
                {
                    // LanguageDetection language = new TextEntityExtraction().GetLanguageForRange(langueRange);
                    LanguageDetection language = new TextEntityExtraction().GetLanguageForRange(langueRange.Region);
                    if (language.language == "fra" /*&& language.confidence > 0.5*/)
                        frMode = true;
                    if (language.language == "eng" && language.confidence > 0.7)    // it thinks arabizi is 'eng' with 0.65 confid
                        frMode = true;

                    // String larabicText = langueRange;
                    String larabicText = langueRange.Region;

                    if (frMode == false)
                        larabicText = new TextConverter().Preprocess_upstream(larabicText);

                    // mark as ignore : url 
                    arabicText = train_markAsIgnore(arabicText);

                    if (frMode == false)
                        arabicText = train_bidict(arabicText);

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

                // String larabicText = langueRange;
                String larabicText = languagesRanges[0].Region;

                if (frMode == false)
                    larabicText = new TextConverter().Preprocess_upstream(larabicText);

                // mark as ignore : url 
                arabicText = train_markAsIgnore(arabicText);

                if (frMode == false)
                    arabicText = train_bidict(arabicText);

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

        private static string train_binggoogle(String arabicText)
        {
            // first pass : correct/translate the original arabizi into msa arabic using big/google apis (to take care of french/english segments in codeswitch arabizi posts)
            return new TranslationTools().CorrectTranslate(arabicText);
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
            // return arabicText;
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

            // return arabicText;
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

            // return arabicText;
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
        /*private void saveserializeM_ARABIZIENTRY(M_ARABIZIENTRY arabiziEntry)
        {
            saveserializeM_ARABIZIENTRY_EFSQL(arabiziEntry);
        }*/

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

        /*private void saveserializeM_ARABICDARIJAENTRY(M_ARABICDARIJAENTRY arabicDarijaEntry, AccessMode accessMode)
        {
            if (accessMode == AccessMode.efsql)
            {
                saveserializeM_ARABICDARIJAENTRY_EFSQL(arabicDarijaEntry);
            }
        }*/

        private void saveserializeM_ARABICDARIJAENTRY_EFSQL(M_ARABICDARIJAENTRY arabicDarijaEntry)
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

        /*private void saveserializeM_ARABICDARIJAENTRY_LATINWORD(M_ARABICDARIJAENTRY_LATINWORD latinWord, AccessMode accessMode)
        {
            if (accessMode == AccessMode.efsql)
                saveserializeM_ARABICDARIJAENTRY_LATINWORD_EFSQL(latinWord);
        }*/

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
            // using (var db = new ArabiziDbContext())
            // {
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
            // }
        }
        #endregion

        #region BACK YARD BO LOAD
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
    }
}
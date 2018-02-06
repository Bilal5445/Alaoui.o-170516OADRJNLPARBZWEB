using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Script.Serialization;
using ArabicTextAnalyzer.Contracts;
using ArabicTextAnalyzer.Domain;
using RestSharp;
using ArabicTextAnalyzer.Domain.Models;
using System.Linq;
using System.Web;
using OADRJNLPCommon.Business;
using static ArabicTextAnalyzer.Business.Provider.RosetteMultiLanguageDetections;
using Newtonsoft.Json.Linq;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TextEntityExtraction : ITextEntityExtraction
    {
        private readonly RestClient client;

        private const string endpoint = "https://api.rosette.com/rest/v1/";
        private const string rosetteApiKey = "ce51b85cd7c17f407f2ab16799896808";

        // DBG
        private const bool doNotUseRosette = true;

        public TextEntityExtraction()
        {
            client = new RestClient(endpoint);
        }

        public List<LanguageRange> GetLanguagesRanges(String source)
        {
            if (doNotUseRosette == true)
            {
                // In this case, we consider the text as one language that should be considered as arabizi (no FR)
                var languageRanges = new List<LanguageRange>();
                languageRanges.Add(new LanguageRange
                {
                    Language = new LanguageDetection
                    {
                        language = "arz",
                        confidence = 1.0
                    },
                    Region = source
                });
                return languageRanges;
            }

            //
            var request = new RestRequest("language", Method.POST);

            request.AddHeader("X-RosetteAPI-Key", rosetteApiKey);
            request.AddHeader("Accept", "application/json");

            var content = new JavaScriptSerializer().Serialize(
                    new
                    {
                        content = source,
                        options = new
                        {
                            multilingual = true
                        }
                    });

            request.AddParameter("application/json", content, ParameterType.RequestBody);

            var response = client.Execute(request);

            //
            var ranges = new List<String>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                RosetteMultiLanguageDetections responseObject = new JavaScriptSerializer().Deserialize<RosetteMultiLanguageDetections>(response.Content);

                //
                return responseObject.regionalDetections.Select(m => new LanguageRange
                {
                    Region = m.region,
                    Language = m.languages[0]
                }).ToList();
            }
            else
            {
                JObject jResponseContent = JObject.Parse(response.Content);
                var code = Convert.ToString(jResponseContent["code"]);
                if (code == "forbidden" || code == "tooManyRequests")
                {
                    // Probably you have either provided an invalid API key, or are not authorized to call the endpoint, or have exceeded the daily or monthly API call limits
                    // In this case, we consider the text as one language that should be considered as arabizi (no FR)
                    var languageRanges = new List<LanguageRange>();
                    languageRanges.Add(new LanguageRange
                    {
                        Language = new LanguageDetection
                        {
                            language = "arz",
                            confidence = 1.0
                        },
                        Region = source
                    });
                    return languageRanges;
                }
                else
                    throw new Exception(response.Content);
            }
        }

        public LanguageDetection GetLanguageForRange(String source)
        {
            //
            var request = new RestRequest("language", Method.POST);

            request.AddHeader("X-RosetteAPI-Key", rosetteApiKey);
            request.AddHeader("Accept", "application/json");

            var content = new JavaScriptSerializer().Serialize(
                    new
                    {
                        content = source,
                    });

            request.AddParameter("application/json", content, ParameterType.RequestBody);

            var response = client.Execute(request);

            //
            var ranges = new List<String>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                RosetteMultiLanguageDetections responseObject = new JavaScriptSerializer().Deserialize<RosetteMultiLanguageDetections>(response.Content);

                return responseObject.languageDetections[0];
            }

            return null;
        }

        public IEnumerable<TextEntity> GetEntities(string source)
        {
            var request = new RestRequest("entities", Method.POST);

            request.AddHeader("X-RosetteAPI-Key", rosetteApiKey);
            request.AddHeader("Accept", "application/json");

            var content = new JavaScriptSerializer().Serialize(
                    new
                    {
                        content = source
                    });

            request.AddParameter("application/json", content, ParameterType.RequestBody);

            var response = client.Execute(request);

            // MC301117 rosette can return badrequest if it does not recognize the language : {"code":"unsupportedLanguage","message":"Language swe not supported"}
            // It happenned with "macharmla"
            List<TextEntity> returnValue = new List<TextEntity>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseObject = new JavaScriptSerializer().Deserialize<RosetteEntityResponse>(response.Content);

                foreach (var rosetteEntity in responseObject.Entities)
                {
                    returnValue.Add(new TextEntity
                    {
                        Count = rosetteEntity.Count,
                        EntityId = rosetteEntity.EntityId,
                        Mention = rosetteEntity.Mention,
                        Normalized = rosetteEntity.Normalized,
                        Type = rosetteEntity.Type
                    });
                }
            }

            return returnValue;
        }

        public List<M_ARABICDARIJAENTRY_TEXTENTITY> NerManualExtraction(String arabicText, IEnumerable<TextEntity> entities, Guid arabicDarijaEntry_ID_ARABICDARIJAENTRY, 
            Action<M_ARABICDARIJAENTRY_TEXTENTITY, AccessMode> saveserializeM_ARABICDARIJAENTRY_TEXTENTITY,
            AccessMode accessMode
            )
        {
            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = new List<M_ARABICDARIJAENTRY_TEXTENTITY>();

            // clean post-rosette
            var lentities = NerRosetteClean(entities);

            // NER manual extraction
            new TextFrequency().GetManualEntities(arabicText, lentities);

            // clean 3 post rosette & manual ners : drop self containing
            foreach (var entity in lentities)
            {
                var entitiesToDrop = entities.ToList().FindAll(m => m.Mention != entity.Mention && m.Mention.Contains(entity.Mention));
                foreach (var entityToDrop in entitiesToDrop)
                {
                    entityToDrop.Type = "TODROP";
                }
            }
            lentities.RemoveAll(m => m.Type == "TODROP");

            // Saving
            foreach (var entity in lentities)
            {
                var textEntity = new M_ARABICDARIJAENTRY_TEXTENTITY
                {
                    ID_ARABICDARIJAENTRY_TEXTENTITY = Guid.NewGuid(),
                    ID_ARABICDARIJAENTRY = arabicDarijaEntry_ID_ARABICDARIJAENTRY,
                    TextEntity = entity
                };

                //
                textEntities.Add(textEntity);

                // Save to Serialization
                saveserializeM_ARABICDARIJAENTRY_TEXTENTITY(textEntity, accessMode);
            }

            //
            return textEntities;
        }

        public List<M_ARABICDARIJAENTRY_TEXTENTITY> NerManualExtraction_uow(String arabicText, IEnumerable<TextEntity> entities, Guid arabicDarijaEntry_ID_ARABICDARIJAENTRY,
            Action<M_ARABICDARIJAENTRY_TEXTENTITY, ArabiziDbContext, bool> saveserializeM_ARABICDARIJAENTRY_TEXTENTITY_EFSQL_uow,
            ArabiziDbContext db,
            bool isEndOfScope
            )
        {
            List<M_ARABICDARIJAENTRY_TEXTENTITY> textEntities = new List<M_ARABICDARIJAENTRY_TEXTENTITY>();

            // clean post-rosette
            var lentities = NerRosetteClean(entities);

            // NER manual extraction
            new TextFrequency().GetManualEntities(arabicText, lentities);

            // clean 3 post rosette & manual ners : drop self containing
            foreach (var entity in lentities)
            {
                var entitiesToDrop = entities.ToList().FindAll(m => m.Mention != entity.Mention && m.Mention.Contains(entity.Mention));
                foreach (var entityToDrop in entitiesToDrop)
                {
                    entityToDrop.Type = "TODROP";
                }
            }
            lentities.RemoveAll(m => m.Type == "TODROP");

            // Saving
            foreach (var entity in lentities)
            {
                var textEntity = new M_ARABICDARIJAENTRY_TEXTENTITY
                {
                    ID_ARABICDARIJAENTRY_TEXTENTITY = Guid.NewGuid(),
                    ID_ARABICDARIJAENTRY = arabicDarijaEntry_ID_ARABICDARIJAENTRY,
                    TextEntity = entity
                };

                //
                textEntities.Add(textEntity);

                // Save to Serialization
                saveserializeM_ARABICDARIJAENTRY_TEXTENTITY_EFSQL_uow(textEntity, db, isEndOfScope);
            }

            //
            return textEntities;
        }

        public List<TextEntity> NerRosetteClean(IEnumerable<TextEntity> entities)
        {
            var lentities = entities.ToList();

            // clean rosette ners : type url (IDENTIFIER:URL)
            lentities.RemoveAll(m => m.Type == "IDENTIFIER:URL");

            // clean 2 rosette ners : drop self containing
            // foreach (var entity in entities)
            foreach (var entity in lentities)
            {
                // var entitiesToDrop = entities.ToList().FindAll(m => m.Mention != entity.Mention && m.Mention.Contains(entity.Mention));
                var entitiesToDrop = lentities.FindAll(m => m.Mention != entity.Mention && m.Mention.Contains(entity.Mention));
                foreach (var entityToDrop in entitiesToDrop)
                {
                    entityToDrop.Type = "TODROP";
                }
            }
            // var lentities = entities.ToList();
            lentities.RemoveAll(m => m.Type == "TODROP");

            // clean 3 rosette ners : drop entities from exlusion files (ex:allah : irrelevant for us)
            var textFrequency = new TextFrequency();
            foreach (var entityToDrop in lentities)
            {
                if (textFrequency.NotNERContainsWord(entityToDrop.Mention))
                {
                    entityToDrop.Type = "TODROP";
                }
            }
            lentities.RemoveAll(m => m.Type == "TODROP");

            //
            return lentities;
        }
    }
}
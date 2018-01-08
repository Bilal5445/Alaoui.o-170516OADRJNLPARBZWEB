﻿using System;
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

namespace ArabicTextAnalyzer.Business.Provider
{
    public class TextEntityExtraction : ITextEntityExtraction
    {
        private readonly RestClient client;

        private const string endpoint = "https://api.rosette.com/rest/v1/";

        public TextEntityExtraction()
        {
            client = new RestClient(endpoint);
        }

        public IEnumerable<TextEntity> GetEntities(string source)
        {
            var request = new RestRequest("entities", Method.POST);

            request.AddHeader("X-RosetteAPI-Key", "5021a32a1791e801856d0f4d01f694be");
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
            // clean 2 rosette ners : drop self containing
            foreach (var entity in entities)
            {
                var entitiesToDrop = entities.ToList().FindAll(m => m.Mention != entity.Mention && m.Mention.Contains(entity.Mention));
                foreach (var entityToDrop in entitiesToDrop)
                {
                    entityToDrop.Type = "TODROP";
                }
            }
            var lentities = entities.ToList();
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
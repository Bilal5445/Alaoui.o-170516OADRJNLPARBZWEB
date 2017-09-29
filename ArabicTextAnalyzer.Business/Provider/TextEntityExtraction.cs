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

            List<TextEntity> returnValue = null;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseObject = new JavaScriptSerializer().Deserialize<RosetteEntityResponse>(response.Content);

                returnValue = new List<TextEntity>();

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

        public void NerManualExtraction(String arabicText, IEnumerable<TextEntity> entities, Guid arabicDarijaEntry_ID_ARABICDARIJAENTRY, HttpServerUtilityBase Server)
        {
            // clean post-rosette
            var lentities = NerRosetteClean(entities);

            // NER manual extraction
            /*foreach (var word in arabicText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                String typeEntity;
                if (new TextFrequency().NERStartsWithWord_brands(word, out typeEntity))
                {
                    // add only if not already in entities
                    // otherwise increment
                    TextEntity existingEntity = lentities.FirstOrDefault(m => m.Mention == word);
                    if (existingEntity == null)
                    {
                        lentities.Add(new TextEntity
                        {
                            Count = 1,
                            Mention = word,
                            Type = typeEntity
                        });
                    }
                    else
                    {
                        existingEntity.Count++;
                    }
                }
            }*/
            new TextFrequency().GetManualEntities(arabicText, lentities);

            // Saving
            foreach (var entity in lentities)
            {
                var textEntity = new M_ARABICDARIJAENTRY_TEXTENTITY
                {
                    ID_ARABICDARIJAENTRY_TEXTENTITY = Guid.NewGuid(),
                    ID_ARABICDARIJAENTRY = arabicDarijaEntry_ID_ARABICDARIJAENTRY,
                    TextEntity = entity
                };

                // Save to Serialization
                var path = Server.MapPath("~/App_Data/data_M_ARABICDARIJAENTRY_TEXTENTITY.txt");
                new TextPersist().Serialize(textEntity, path);
            }
        }

        public List<TextEntity> NerRosetteClean(IEnumerable<TextEntity> entities)
        {
            // clean 1 rosette ners : reset counter
            foreach (var entity in entities)
                entity.Count = 0;

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
            // lentities.RemoveAll(m => m.Mention == "الله");
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
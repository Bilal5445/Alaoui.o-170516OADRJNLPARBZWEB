using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Script.Serialization;
using ArabicTextAnalyzer.Contracts;
using ArabicTextAnalyzer.Domain;
using ArabicTextAnalyzer.Models;
using RestSharp;
using RestSharp.Authenticators;

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
                        content =
                            "سمعت هاد شنو ك كل سمعت هاد الهضرة سر تاخدهم سمعتني بركة من تخربيق شفت هادشي واش كتبغيني واش نتا حمص هادشي كاي ferahh bezzaf , و شفت ما steaamaltch l’7 و l’3 باش nt"
                    });


            request.AddParameter("application/json", content, ParameterType.RequestBody);


            var response = client.Execute(request);

            //var responseObject = new JavaScriptSerializer().Deserialize<IBMSentimentResponse>(response.Content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseObject = new JavaScriptSerializer().Deserialize<RosetteEntityResponse>(response.Content);
                var returnValue = new List<TextEntity>();

                foreach (var rosetteEntity in responseObject.Entities)
                {
                    returnValue.Add(new TextEntity
                    {
                       s
                    });
                }
            }

            return null;
        }
    }
}
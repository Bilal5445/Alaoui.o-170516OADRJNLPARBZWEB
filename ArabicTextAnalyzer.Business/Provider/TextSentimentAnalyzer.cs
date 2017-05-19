using System;
using System.Net;
using System.Net.Http.Headers;
using ArabicTextAnalyzer.Contracts;
using ArabicTextAnalyzer.Domain;
using ArabicTextAnalyzer.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Web.Script.Serialization;

namespace ArabicTextAnalyzer.Business.Provider 
{
    public class TextSentimentAnalyzer : ITextSentimentAnalyzer
    {
        private readonly RestClient client;

        private const string endpoint = "https://gateway.watsonplatform.net/";

        public TextSentimentAnalyzer()
        {
            client = new RestClient(endpoint);
        }

        public TextSentiment GetSentiment(string source)
        {
            var request = new RestRequest("natural-language-understanding/api/v1/analyze", Method.GET);

            client.Authenticator = new HttpBasicAuthenticator("52cef037-89f1-4621-9def-4ab4c618d83c", "tQ57OyQgJlQ6");

            request.AddQueryParameter("version", "2017-02-27");
            request.AddQueryParameter("text", "صرخة سيئة قبيح جد");
            request.AddQueryParameter("features", "sentiment");

            var response = client.Execute(request);

            var responseObject = new JavaScriptSerializer().Deserialize<IBMSentimentResponse>(response.Content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return new TextSentiment
                {
                    Label = responseObject.Sentiment.Document.Label,
                    Score = responseObject.Sentiment.Document.Score
                };
            }

            return null;
        }
    }
}
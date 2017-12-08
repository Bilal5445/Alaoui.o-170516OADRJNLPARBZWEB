using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ArabicTextAnalyzer.Business.Provider;
using ArabicTextAnalyzer.Contracts;

namespace ArabicTextAnalyzer.Business
{
    public class AnalyzeTextAppService
    {
        private readonly ITextConverter textConveter;

        private readonly ITextEntityExtraction textEntityExtraction;

        private readonly ITextSentimentAnalyzer textSentimentAnalyzer;

        public AnalyzeTextAppService()
        {
            textConveter = new TextConverter();
            textEntityExtraction = new TextEntityExtraction();
            textSentimentAnalyzer = new TextSentimentAnalyzer();
        }
    }
}
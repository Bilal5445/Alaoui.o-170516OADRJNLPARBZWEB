using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//            "{
//  "sentiment": {
//    "document": {
//      "score": -0.799036,
//      "label": "negative"
//    }
//  },
//  "language": "ar"
//}"

namespace ArabicTextAnalyzer.Business.Provider
{
    public class IBMSentimentResponse
    {
        public Sentiment Sentiment { get; set; }
        
        public string Language { get; set; }    
    }

    public class Sentiment
    {
        public Document Document { get; set; }
    }

    public class Document
    {
        public float Score { get; set; }

        public string Label { get; set; }
    }
}

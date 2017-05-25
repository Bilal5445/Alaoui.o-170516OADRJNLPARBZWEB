using System.Collections.Generic;
using ArabicTextAnalyzer.Domain;

namespace ArabicTextAnalyzer.Models
{
    public class TextAnalyze
    {
        public TextSentiment Sentiment { get; set; }

        public List<TextEntity> Entities { get; set; }
    }
}
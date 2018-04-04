using System;
using System.Collections.Generic;
using static ArabicTextAnalyzer.Business.Provider.RosetteMultiLanguageDetections;

namespace ArabicTextAnalyzer.Business.Provider
{
    public class RosetteEntityResponse
    {
        public IEnumerable<RosetteEntity> Entities { get; set; } 
    }

    public class RosetteEntity
    {
        public long Count { get; set; }
        public string EntityId { get; set; }
        public string Mention { get; set; }
        public string Normalized { get; set; }
        public string Type { get; set; }
    }

    public class RosetteMultiLanguageDetections
    {
        public List<LanguageDetection> languageDetections { get; set; }
        public List<RegionalDetection> regionalDetections { get; set; }

        public class LanguageDetection
        {
            public string language { get; set; }
            public double confidence { get; set; }
        }

        /*public class Language
        {
            public string language { get; set; }
            public double confidence { get; set; }
        }*/

        public class RegionalDetection
        {
            public string region { get; set; }
            public List</*Language*/LanguageDetection> languages { get; set; }
        }
    }

    public class LanguageRange
    {
        public String Region { get; set; }
        public LanguageDetection Language { get; set; }
    }
}

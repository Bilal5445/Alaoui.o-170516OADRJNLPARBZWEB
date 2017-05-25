using System;
using System.Collections.Generic;
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
}

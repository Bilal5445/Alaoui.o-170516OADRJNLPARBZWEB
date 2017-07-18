using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabicTextAnalyzer.Models
{
    public class M_ARABICDARIJAENTRY_LATINWORD
    {
        // one-to-many
        public Guid ID_ARABICDARIJAENTRY { get; set; }
        public String LatinWord { get; set; }
    }
}
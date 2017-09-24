using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    public class M_ARABICDARIJAENTRY_LATINWORD
    {
        public Guid ID_ARABICDARIJAENTRY_LATINWORD { get; set; }    // PK
        public Guid ID_ARABICDARIJAENTRY { get; set; }              // FK one-to-many
        public String LatinWord { get; set; }
        public int VariantsCount { get; set; }
        public String MostPopularVariant { get; set; }
        public String Translation { get; set; }
    }
}

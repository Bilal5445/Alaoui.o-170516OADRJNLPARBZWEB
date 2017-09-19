using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    public class M_ARABICDARIJAENTRY_TEXTENTITY
    {
        public Guid ID_ARABICDARIJAENTRY_TEXTENTITY { get; set; }    // PK
        public Guid ID_ARABICDARIJAENTRY { get; set; }              // FK one-to-many
        public TextEntity TextEntity { get; set; }
    }
}

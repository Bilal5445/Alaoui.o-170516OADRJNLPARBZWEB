using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArabicTextAnalyzer.Models
{
    public class M_ARABICDARIJAENTRY
    {
        public Guid ID_ARABICDARIJAENTRY { get; set; }
        public Guid ID_ARABIZIENTRY { get; set; }
        public String ArabicDarijaText { get; set; }
    }
}
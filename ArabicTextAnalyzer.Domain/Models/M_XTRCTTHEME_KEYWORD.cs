using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabicTextAnalyzer.Domain.Models
{
    public class M_XTRCTTHEME_KEYWORD
    {
        public Guid ID_XTRCTTHEME_KEYWORD { get; set; }    // PK
        public Guid ID_XTRCTTHEME { get; set; }              // FK one-to-many
        public String Keyword { get; set; }
    }
}
